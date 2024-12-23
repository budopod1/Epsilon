using System.Text;

namespace Epsilon;
public class NameMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken stoken = tokens[i];
            if (stoken is not TextToken) {
                continue;
            }
            string nameStart = ((TextToken)stoken).GetText();
            if (!Utils.NameStartChars.Contains(nameStart)) {
                continue;
            }
            StringBuilder name = new(nameStart);
            List<IToken> replaced = [stoken];
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken token1) {
                    string text = token1.GetText();

                    if (Utils.NameChars.Contains(text)) {
                        replaced.Add(token);
                        name.Append(text);
                        continue;
                    }
                }
                break;
            }
            List<IToken> replacement = [new Name(name.ToString())];
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
