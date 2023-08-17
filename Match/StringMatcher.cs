using System;
using System.Collections.Generic;

public class StringMatcher : IMatcher {
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            Token token = tokens[i];
            if (!(token is TextToken) || ((TextToken)token).GetText() != "\"") {
                continue;
            }
            
            List<Token> matched = new List<Token>();
            matched.Add(token);
            bool wasBackslash = false;
            for (int j = i + 1; j < tokens.Count; j++) {
                token = tokens[j];
                if (!(token is TextToken)) {
                    continue;
                }
                matched.Add(token);
                string text = ((TextToken)token).GetText();
                if (wasBackslash) {
                    wasBackslash = false;
                } else {
                    if (text == "\\") {
                        wasBackslash = true;
                    } else if (text == "\"") {
                        return new Match(i, j, new List<Token>(),
                                         matched);
                    }
                }
            }
            
        }
        return null;
    }
}
