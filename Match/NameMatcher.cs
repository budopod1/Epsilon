using System;
using System.Collections.Generic;

public class NameMatcher : IMatcher {
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            Token stoken = tokens[i];
            if (!(stoken is TextToken)) {
                continue;
            }
            string name = ((TextToken)stoken).GetText();
            if (!Utils.NameStartChars.Contains(name)) {
                continue;
            }
            List<Token> replaced = new List<Token>();
            replaced.Add(stoken);
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                Token token = tokens[j];
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
            List<Token> replacement = new List<Token>();
            replacement.Add(new Name(name));
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
