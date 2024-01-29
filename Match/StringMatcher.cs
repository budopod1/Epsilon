using System;
using System.Linq;
using System.Collections.Generic;

public class StringMatcher : IMatcher {
    Program program;

    public StringMatcher(Program program) {
        this.program = program;
    }
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (!(token is TextToken) || 
                ((TextToken)token).GetText() != "\"") {
                continue;
            }
            
            List<IToken> matched = new List<IToken>();
            matched.Add(token);
            bool wasBackslash = false;
            for (int j = i + 1; j < tokens.Count; j++) {
                token = tokens[j];;
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
                        string matchedString = String.Join(
                            "", matched.Select(
                                (IToken sub) => ((TextToken)sub).GetText()
                            )
                        );
                        List<IToken> replacement = new List<IToken> {
                            new ConstantValue(
                                StringConstant.FromString(matchedString)
                            )
                        };
                        
                        return new Match(i, j, replacement, matched);
                    }
                }
            }
        }
        return null;
    }
}
