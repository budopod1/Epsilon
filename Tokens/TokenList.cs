using System;
using System.Collections;
using System.Collections.Generic;

public class TokenList : IEnumerator<Token> {
    List<Token> tokens;
    int i = -1;
    
    public TokenList(List<Token> tokens) {
        this.tokens = tokens;
    }

    public bool MoveNext() {
        i++;
        return (i < tokens.Count);
    }

    public void Reset() {
        i = -1;
    }

    public Token Current {
        get {
            return this.tokens[i];
        }
    }

    public void Dispose() {}

    object IEnumerator.Current {
        get {
            return this.Current;
        }
    }
}
