using System;
using System.Collections.Generic;

public class DisposePatternProcessor : IPatternProcessor<List<IToken>> {
    Action<List<IToken>> action;
    
    public DisposePatternProcessor(Action<List<IToken>> action) {
        this.action = action;
    }
    
    public DisposePatternProcessor() {
        this.action = (List<IToken> tokens) => {};
    }

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        action(tokens);
        return new List<IToken>();
    }
}
