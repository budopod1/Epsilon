using System;
using System.Collections.Generic;

public class FunctionArgumentMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            bool first = true;
            int indentCount = 0;
            List<IToken> replaced = new List<IToken>();
            List<IToken> replacementTokens = new List<IToken>();
            for (int j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (!(token is TextToken)) break;
                TextToken ttoken = ((TextToken)token);
                string text = ttoken.GetText();
                if ((text != "<") && first) break;
                if (text == "<") indentCount++;
                if (text == ">") indentCount--;
                if (indentCount == 0) {
                    return new Match(i, j, new List<IToken> {
                        new RawFunctionArgument(replacementTokens)
                    }, replaced);
                }
                if (!first) replacementTokens.Add(token);
                first = false;
            }
        }
        return null;
    }
}
