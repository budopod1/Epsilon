using System;
using System.Collections.Generic;

public class TreeToken : IToken {
    public List<IToken> tokens;
    
    public TreeToken(List<IToken> tokens) {
        this.tokens = tokens;
    }

    public void Add(IToken token) {
        this.tokens.Add(token);
    }

    public override string ToString() {
        string result = "";
        foreach (IToken token in this.tokens) {
            result += token.ToString();
        }
        return result;
    }
}
