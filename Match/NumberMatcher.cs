using System;
using System.Linq;
using System.Collections.Generic;

public class NumberMatcher : IMatcher {
    Program program;

    public NumberMatcher(Program program) {
        this.program = program;
    }

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            List<IToken> replaced = new List<IToken>();
            bool dot = false;
            bool anyMatch = false;
            bool isNegative = false;
            bool content = false;
            int j;
            for (j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (!(token is TextToken)) {
                    break;
                }
                string digit = ((TextToken)token).GetText();
                bool foundMatch = false;
                if (digit == "-" && !anyMatch) {
                    foundMatch = true;
                    isNegative = true;
                } else if ("1234567890".Contains(digit)) {
                    foundMatch = true;
                    content = true;
                } else if (digit == "." && !dot) {
                    dot = true;
                    foundMatch = true;
                }
                if (!foundMatch) {
                    break;
                }
                anyMatch |= foundMatch;
                replaced.Add(token);
            }
            if (anyMatch && content) {
                string matchedString = String.Join(
                    "", replaced.Select(
                        sub => ((TextToken)sub).GetText()
                    )
                );
                
                IConstant constant;
                if (dot) {
                    constant = FloatConstant.FromString(matchedString);
                } else if (isNegative) {
                    constant = IntConstant.FromString(matchedString);
                } else {
                    constant = UnsignedIntConstant.FromString(matchedString);
                }
                
                List<IToken> replacement = new List<IToken> {
                    new ConstantValue(constant)
                };

                return new Match(i, j-1, replacement, replaced);
            }
        }
        return null;
    }
}
