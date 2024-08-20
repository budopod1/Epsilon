using System;
using System.Reflection;
using System.Collections.Generic;

public class Wrapper2PatternProcessor : IPatternProcessor<List<IToken>> {
    readonly Type wrapper;
    readonly IPatternProcessor<List<IToken>> subprocessor;

    public Wrapper2PatternProcessor(IPatternProcessor<List<IToken>> subprocessor,
                                   Type wrapper) {
        this.wrapper = wrapper;
        this.subprocessor = subprocessor;
    }

    public Wrapper2PatternProcessor(Type wrapper) {
        this.wrapper = wrapper;
        this.subprocessor = null;
    }

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        List<IToken> ntokens = tokens;
        if (subprocessor != null) {
            ntokens = subprocessor.Process(tokens, start, end);
        }
        IToken result = (IToken)Activator.CreateInstance(
            wrapper, ntokens.ToArray()
        );
        return [result];
    }
}
