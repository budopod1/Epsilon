using System;
using System.Reflection;
using System.Collections.Generic;

public class WrapperPatternProcessor : IPatternProcessor<List<IToken>> {
    Type wrapper;
    IPatternProcessor<List<IToken>> subprocessor;
    
    public WrapperPatternProcessor(IPatternProcessor<List<IToken>> subprocessor,
                                   Type wrapper) {
        this.wrapper = wrapper;
        this.subprocessor = subprocessor;
    }

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        IToken result = (IToken)Activator.CreateInstance(
            wrapper, new object[] {subprocessor.Process(tokens, start, end)}
        );
        return new List<IToken> {result};
    }
}
