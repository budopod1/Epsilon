using System;
using System.Collections.Generic;

public class DisposePatternProcessor : IPatternProcessor<List<IToken>> {
    Action<List<IToken>> action;

    public DisposePatternProcessor(Action<List<IToken>> action) {
        this.action = action;
    }

    public DisposePatternProcessor() {
        this.action = null;
    }

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        if (action != null) action(tokens);
        return new List<IToken>();
    }
}
