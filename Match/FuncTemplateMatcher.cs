using System;
using System.Collections.Generic;

public class FuncTemplateMatcher : IMatcher {
    char startChar;
    char endMarkerChar;
    Type holderType;
    
    public FuncTemplateMatcher(char start, char endMarker, Type holder) {
        startChar = start;
        endMarkerChar = endMarker;
        holderType = holder;
    }
    
    public Match Match(IToken tokens_) {
        if (!(tokens_ is TreeToken)) return null;
        TreeToken tokens = (TreeToken)tokens_;
        int i = -1;
        foreach (IToken stoken in tokens) {
            i += 1;
            if (!(stoken is TextToken)) {
                continue;
            }
            if (((TextToken)stoken).Text != startChar.ToString()) {
                continue;
            }
            List<IToken> replaced = new List<IToken>();
            replaced.Add(stoken);
            int j;
            string template = "";
            for (j = i+1; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken) {
                    string text = ((TextToken)token).Text;
                    
                    if (text != endMarkerChar.ToString()) {
                        replaced.Add(token);
                        template += text;
                        continue;
                    }
                }
                break;
            }
            List<IToken> replacement = new List<IToken>();
            replacement.Add(new FuncTemplate(template.Trim()));
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
