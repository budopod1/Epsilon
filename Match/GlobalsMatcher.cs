using System;
using System.Collections.Generic;

public class GlobalsMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        bool wasNL = true;
        for (int i = 0; i < tokens.Count-2; i++) {
            IToken stoken = tokens[i];
            TextToken sttoken = stoken as TextToken;
            if (sttoken != null) {
                string stext = sttoken.GetText();
                if (stext == "\n") {
                    wasNL = true;
                    continue;
                } else if (wasNL && stext == "#") {
                    wasNL = false;
                    Name nntoken = tokens[i+1] as Name;
                    if (nntoken == null || nntoken.GetValue() != "global") continue;
                    TextToken wtoken = tokens[i+2] as TextToken;
                    if (wtoken == null || !Utils.Whitespace.Contains(wtoken.GetText())) continue;
                    List<IToken> matched = new List<IToken> {stoken, nntoken, wtoken};
                    List<IToken> declaration = new List<IToken>();
                    for (int j = 3; j + i < tokens.Count; j++) {
                        IToken token = tokens[i+j];
                        TextToken ttoken = token as TextToken;
                        if (ttoken?.GetText() == ";") {
                            return new Match(i, i+j, new List<IToken> {
                                new RawGlobal(declaration)
                            }, matched);
                        } else {
                            matched.Add(token);
                            declaration.Add(token);
                        }
                    }
                }
            }
            wasNL = false;
        }
        return null;
    }
}
