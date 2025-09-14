namespace Epsilon;
public class GlobalsMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        bool wasNL = true;
        for (int i = 0; i < tokens.Count-2; i++) {
            IToken stoken = tokens[i];
            if (stoken is TextToken sttoken) {
                string stext = sttoken.GetText();
                if (stext == "\n") {
                    wasNL = true;
                    continue;
                } else if (wasNL && stext == "%") {
                    wasNL = false;
                    if (tokens[i + 1] is not Name nntoken
                        || nntoken.GetValue() != "global") continue;
                    if (tokens[i + 2] is not TextToken wtoken
                        || !Utils.Whitespace.Contains(wtoken.GetText())) continue;
                    List<IToken> matched = [stoken, nntoken, wtoken];
                    List<IToken> declaration = [];
                    for (int j = 3; j + i < tokens.Count; j++) {
                        IToken token = tokens[i+j];
                        TextToken ttoken = token as TextToken;
                        if (ttoken?.GetText() == ";") {
                            return new Match(i, i+j, [
                                new RawGlobal(declaration)
                            ], matched);
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
