using System;
using System.Reflection;
using System.Collections.Generic;

public class SplitTokensProcessor : IPatternProcessor<List<IToken>> {
    Type wrapper;
    IPatternSegment seperator;
    IPatternProcessor<List<IToken>> subprocessor;
    
    public SplitTokensProcessor(IPatternProcessor<List<IToken>> subprocessor,
                                IPatternSegment seperator, Type wrapper) {
        this.wrapper = wrapper;
        this.seperator = seperator;
        this.subprocessor = subprocessor;
    }
    
    public SplitTokensProcessor(IPatternSegment seperator, Type wrapper) {
        this.wrapper = wrapper;
        this.seperator = seperator;
        this.subprocessor = null;
    }
    
    public List<IToken> Process(List<IToken> tokens_, int start, int end) {
        List<IToken> tokens = tokens_;
        if (subprocessor != null) {
            tokens = subprocessor.Process(tokens, start, end);
        }
        SplitTokensParser parser = new SplitTokensParser(
            seperator, true
        );
        List<List<IToken>> split = parser.Parse(tokens);
        List<IToken> result = new List<IToken>();
        foreach (List<IToken> section in split) {
            result.Add((IToken)Activator.CreateInstance(
                wrapper, new object[] {section}
            ));
        }
        return result;
    }
}
