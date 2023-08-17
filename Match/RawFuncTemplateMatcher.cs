using System;
using System.Collections.Generic;

public class RawFuncTemplateMatcher : IMatcher {
    char startChar;
    char endMarkerChar;
    Type holderType;
    
    public RawFuncTemplateMatcher(char start, char endMarker, Type holder) {
        startChar = start;
        endMarkerChar = endMarker;
        holderType = holder;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            Token stoken = tokens[i];
            if (!(stoken is TextToken)) {
                continue;
            }
            if (((TextToken)stoken).GetText() != startChar.ToString()) {
                continue;
            }
            List<Token> replaced = new List<Token>();
            List<Token> replacementTokens = new List<Token>();
            replaced.Add(stoken);
            int j;
            for (j = i+1; j < tokens.Count; j++) {
                Token token = tokens[j];
                if (token is TextToken) {
                    string text = ((TextToken)token).GetText();
                    
                    if (text != endMarkerChar.ToString()) {
                        replaced.Add(token);
                        if (text.Trim().Length > 0) {
                            replacementTokens.Add(token);
                        }
                        continue;
                    }
                }
                break;
            }
            List<Token> replacement = new List<Token>();
            // replace with reflection?
            replacement.Add(new RawFuncTemplate(replacementTokens)); 
            return new Match(i, j-1, replacement, replaced);
        }
        return null;
    }
}
