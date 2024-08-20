using System;
using System.Collections.Generic;

public class FuncPatternProcessor<T> : IPatternProcessor<T> {
    readonly Func<List<IToken>, int, int, T> func;

    public FuncPatternProcessor(Func<List<IToken>, int, int, T> func) {
        this.func = func;
    }

    public FuncPatternProcessor(Func<List<IToken>, T> func) {
        this.func = (List<IToken> token, int start, int end) => func(token);
    }

    public T Process(List<IToken> tokens, int start, int end) {
        return func(tokens, start, end);
    }
}
