using System;
using System.Collections.Generic;

public class IntMatcher : IMatcher {
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            List<IToken> replaced = new List<IToken>();
            bool anyMatch = false;
            int j;
            for (j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (!(token is TextToken)) {
                    break;
                }
                bool foundMatch = false;
                string digit = ((TextToken)token).Text;
                if (digit == "-" && !anyMatch) {
                    foundMatch = true;
                } else if ("1234567890".Contains(digit)) {
                    foundMatch = true;
                }
                anyMatch |= foundMatch;
                if (!foundMatch) {
                    break;
                }
                replaced.Add(token);
            }
            if (anyMatch) {
                return new Match(i, j-1, replaced, new List<IToken>());
            }
        }
        return null;
    }
}
