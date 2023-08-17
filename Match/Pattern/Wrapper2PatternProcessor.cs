using System;
using System.Reflection;
using System.Collections.Generic;

public class Wrapper2PatternProcessor : IPatternProcessor<List<Token>> {
    Type wrapper;
    IPatternProcessor<List<Token>> subprocessor;
    
    public Wrapper2PatternProcessor(IPatternProcessor<List<Token>> subprocessor,
                                   Type wrapper) {
        this.wrapper = wrapper;
        this.subprocessor = subprocessor;
    }

    public List<Token> Process(List<Token> tokens, int start, int end) {
        Token result = (Token)Activator.CreateInstance(
            wrapper, subprocessor.Process(tokens, start, end).ToArray()
        );
        return new List<Token> {result};
    }
}
