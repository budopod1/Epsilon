using System;
using System.Reflection;
using System.Collections.Generic;

public class SplitTokensPatternProcessor : IPatternProcessor<List<IToken>> {
    readonly Type wrapper;
    readonly IPatternSegment seperator;
    readonly IPatternProcessor<List<IToken>> subprocessor;

    public SplitTokensPatternProcessor(IPatternProcessor<List<IToken>> subprocessor,
                                IPatternSegment seperator, Type wrapper) {
        this.wrapper = wrapper;
        this.seperator = seperator;
        this.subprocessor = subprocessor;
    }

    public SplitTokensPatternProcessor(IPatternSegment seperator, Type wrapper) {
        this.wrapper = wrapper;
        this.seperator = seperator;
        this.subprocessor = null;
    }

    public List<IToken> Process(List<IToken> tokens_, int start, int end) {
        List<IToken> tokens = tokens_;
        if (subprocessor != null) {
            tokens = subprocessor.Process(tokens, start, end);
        }
        SplitTokensParser parser = new(
            seperator, true
        );
        List<List<IToken>> split = parser.Parse(tokens);
        List<IToken> result = [];
        foreach (List<IToken> section in split) {
            IToken token = (IToken)Activator.CreateInstance(
                wrapper, new object[] {section}
            );
            token.span = TokenUtils.MergeSpans(section);
            result.Add(token);
        }
        return result;
    }
}
