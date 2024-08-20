using System;
using System.Collections.Generic;

public class FunctionArgumentMatcher : IMatcher {
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            bool first = true;
            int indentCount = 0;
            List<IToken> replaced = [];
            List<IToken> replacementTokens = [];
            for (int j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                if (token is TextToken ttoken) {
                    string text = ttoken.GetText();
                    if ((text != "<") && first) break;
                    if (text == "<") indentCount++;
                    if (text == ">") indentCount--;
                    if (indentCount == 0) {
                        return new Match(i, j, [
                            new RawFunctionArgument(replacementTokens)
                        ], replaced);
                    }
                } else if (first) {
                    break;
                }
                if (!first) replacementTokens.Add(token);
                first = false;
            }
        }
        return null;
    }
}
