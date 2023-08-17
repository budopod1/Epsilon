using System;
using System.Reflection;
using System.Collections.Generic;

public class WrapperPatternProcessor : IPatternProcessor<List<Token>> {
    Type wrapper;
    IPatternProcessor<List<Token>> subprocessor;
    
    public WrapperPatternProcessor(IPatternProcessor<List<Token>> subprocessor,
                                   Type wrapper) {
        this.wrapper = wrapper;
        this.subprocessor = subprocessor;
    }

    public List<Token> Process(List<Token> tokens, int start, int end) {
        Token result = (Token)Activator.CreateInstance(
            wrapper, new object[] {subprocessor.Process(tokens, start, end)}
        );
        return new List<Token> {result};
    }
}
