using System;
using System.Collections.Generic;

public class MatcherPatternProcessor : IPatternProcessor<Match> {
    IPatternProcessor<List<IToken>> subprocessor;

    public MatcherPatternProcessor(IPatternProcessor<List<IToken>> subprocessor) {
        this.subprocessor = subprocessor;
    }
    
    public Match Process(List<IToken> tokens, int start, int end) {
        return new Match(start, end, subprocessor.Process(tokens, start, end), tokens);
    }
}
