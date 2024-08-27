public class ImportMatcher : IMatcher {
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
                    if (nntoken == null || nntoken.GetValue() != "import") continue;
                    TextToken wtoken = tokens[i+2] as TextToken;
                    if (wtoken == null || !Utils.Whitespace.Contains(wtoken.GetText())) continue;
                    List<IToken> matched = [stoken, nntoken, wtoken];
                    List<string> path = [""];
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
                            return new Match(i, i+j, [
                                new Import(path)
                            ], matched);
                        } else {
                            break;
                        }
                    }
                }
            }
            wasNL = false;
        }
        return null;
    }
}
