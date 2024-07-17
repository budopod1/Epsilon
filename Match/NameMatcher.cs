using System;
using System.Collections.Generic;

public class NameMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken stoken = tokens[i];
            if (!(stoken is TextToken)) {
                continue;
            }
            string name = ((TextToken)stoken).GetText();
            if (!Utils.NameStartChars.Contains(name)) {
                continue;
            }
            List<IToken> replaced = new List<IToken>();
            replaced.Add(stoken);
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken) {
                    string text = ((TextToken)token).GetText();

                    if (Utils.NameChars.Contains(text)) {
                        replaced.Add(token);
                        name += text;
                        continue;
                    }
                }
                break;
            }
            List<IToken> replacement = new List<IToken>();
            if (name.StartsWith("___")) {
                throw new SyntaxErrorException(
                    "Names starting with '___' are reserved",
                    TokenUtils.MergeSpans(replaced)
                );
            }
            replacement.Add(new Name(name));
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
