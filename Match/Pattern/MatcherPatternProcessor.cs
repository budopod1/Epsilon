using System;
using System.Collections.Generic;

public class MatcherPatternProcessor : PatternProcessor<Match> {
    PatternProcessor<List<IToken>> subprocessor;

    public MatcherPatternProcessor(PatternProcessor<List<IToken>> subprocessor) {
        this.subprocessor = subprocessor;
    }
    
    protected override Match Process(List<IToken> tokens) {
        throw new NotSupportedException(
            "MatcherPatternProcessor does not support the Process(List<IToken>) method"
        );
    }
    
    public override Match Process(List<IToken> tokens, int start, int end) {
        return new Match(start, end, subprocessor.Process(tokens, start, end), tokens);
    }
}
