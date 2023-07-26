using System;
using System.Collections.Generic;

public abstract class PatternProcessor<T> {
    protected abstract T Process(List<IToken> tokens);
    
    public virtual T Process(List<IToken> tokens, int start, int end) {
        return Process(tokens);
    }
}
