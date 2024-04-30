using System;
using System.Collections.Generic;

public class ImportMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        bool wasNL = true;
        for (int i = 0; i < tokens.Count-2; i++) {
            IToken stoken = tokens[i];
            TextToken sttoken = stoken as TextToken;
            if (sttoken != null) {
                string stext = sttoken.GetText();
                if (wasNL && stext == "#") {
                    Name nntoken = tokens[i+1] as Name;
                    if (nntoken != null && nntoken.GetValue() == "import") {
                        List<IToken> matched = new List<IToken> {stoken, nntoken};
                        List<string> path = new List<string> {""};
                        bool anyContent = false;
                        bool wasName = false;
                        for (int j = 3; j + i < tokens.Count; j++) {
                            IToken token = tokens[i+j];
                            TextToken ttoken = token as TextToken;
                            Name ntoken = token as Name;
                            string text = ttoken?.GetText();
                            if (text == ".") {
                                matched.Add(token);
                                path.Add("");
                                wasName = false;
                            } else if (ntoken != null) {
                                matched.Add(token);
                                path[path.Count-1] = ntoken.GetValue();
                                anyContent = true;
                                wasName = true;
                            } else if (text == ";" && anyContent) {
                                if (!wasName) {
                                    throw new SyntaxErrorException(
                                        "Import statement path must end with a target name", token
                                    );
                                }
                                return new Match(i, i+j, new List<IToken> {
                                    new Import(path)
                                }, matched);
                            } else {
                                break;
                            }
                        }
                    }
                }
                if (stext == "\n") {
                    wasNL = true;
                    continue;
                }
            }
            wasNL = false;
        }
        return null;
    }
}
