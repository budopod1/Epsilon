using System;
using System.Collections;
using System.Collections.Generic;

public class TokenList : IEnumerator<IToken> {
    List<IToken> tokens;
    int i = -1;
    
    public TokenList(List<IToken> tokens) {
        this.tokens = tokens;
    }

    public bool MoveNext() {
        i++;
        return (i < tokens.Count);
    }

    public void Reset() {
        i = -1;
    }

    public IToken Current {
        get {
            return tokens[i];
        }
    }

    public void Dispose() {}

    object IEnumerator.Current {
        get {
            return Current;
        }
    }
}
