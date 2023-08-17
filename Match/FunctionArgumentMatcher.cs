using System;
using System.Collections.Generic;

public class FunctionArgumentMatcher : IMatcher {
    string start;
    string end;
    Type holderType;
    
    public FunctionArgumentMatcher(string start, string end, Type holder) {
        this.start = start;
        this.end = end;
        holderType = holder;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            bool first = true;
            int indentCount = 0;
            List<Token> replaced = new List<Token>();
            List<Token> replacementTokens = new List<Token>();
            for (int j = i; j < tokens.Count; j++) {
                Token token = tokens[j];
                if (!(token is TextToken)) break;
                TextToken ttoken = ((TextToken)token);
                string text = ttoken.GetText();
                if ((text != start) && first) break;
                if (text == start) indentCount++;
                if (text == end) indentCount--;
                if (indentCount == 0) {
                    Token holder = (Token)Activator.CreateInstance(
                        holderType, new object[] {replacementTokens}
                    );
                    return new Match(i, j, new List<Token> {holder}, replaced);
                }
                if (!first) replacementTokens.Add(token);
                first = false;
            }
        }
        return null;
    }
}
