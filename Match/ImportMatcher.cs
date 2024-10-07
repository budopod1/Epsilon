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
                    if (tokens[i + 1] is not Name nntoken
                        || nntoken.GetValue() != "import") continue;
                    if (tokens[i + 2] is not TextToken wtoken
                        || !Utils.Whitespace.Contains(wtoken.GetText())) continue;
                    List<IToken> matched = [stoken, nntoken, wtoken];
                    List<string> path = [""];
                    bool anyContent = false;
                    bool wasName = false;
                    for (int j = 3; j + i < tokens.Count; j++) {
                        IToken token = tokens[i+j];
                        TextToken ttoken = token as TextToken;
                        string text = ttoken?.GetText();
                        if (text == ".") {
                            matched.Add(token);
                            path.Add("");
                            wasName = false;
                        } else if (token is Name ntoken) {
                            matched.Add(token);
                            path[^1] = ntoken.GetValue();
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
