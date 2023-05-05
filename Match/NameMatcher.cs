using System;
using System.Collections.Generic;

public class NameMatcher : IMatcher {
    public Match Match(IToken tokens_) {
        if (!(tokens_ is TreeToken)) return null;
        TreeToken tokens = (TreeToken)tokens_;
        int i = -1;
        foreach (IToken stoken in tokens) {
            i += 1;
            if (!(stoken is TextToken)) {
                continue;
            }
            string name = ((TextToken)stoken).Text;
            string validS = "_"+Utils.Uppercase+Utils.Lowercase;
            if (!validS.Contains(name)) {
                continue;
            }
            List<IToken> replaced = new List<IToken>();
            replaced.Add(stoken);
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken) {
                    string text = ((TextToken)token).Text;
                    
                    string valid = "_"+Utils.Uppercase;
                    valid += Utils.Lowercase+Utils.Numbers;
                    if (valid.Contains(text)) {
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
