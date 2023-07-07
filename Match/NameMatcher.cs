using System;
using System.Collections.Generic;

public class NameMatcher : IMatcher {
    public Match Match(TreeToken tokens) {
        int i = -1;
        foreach (IToken stoken in tokens) {
            i += 1;
            if (!(stoken is TextToken)) {
                continue;
            }
            string name = ((TextToken)stoken).Text;
            if (!Utils.NameStartChars.Contains(name)) {
                continue;
            }
            List<IToken> replaced = new List<IToken>();
            replaced.Add(stoken);
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken) {
                    string text = ((TextToken)token).Text;
                    
                    if (Utils.NameChars.Contains(text)) {
                        replaced.Add(token);
                        name += text;
                        continue;
                    }
                }
                break;
            }
            List<IToken> replacement = new List<IToken>();
            replacement.Add(new Name(name));
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
