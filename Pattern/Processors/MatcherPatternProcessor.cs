using System;
using System.Collections.Generic;

public class MatcherPatternProcessor : IPatternProcessor<Match> {
    IPatternProcessor<List<IToken>> subprocessor;

    public MatcherPatternProcessor(IPatternProcessor<List<IToken>> subprocessor) {
        this.subprocessor = subprocessor;
    }

    public Match Process(List<IToken> tokens, int start, int end) {
        List<IToken> replacement = subprocessor.Process(tokens, start, end);
        if (replacement == null) return null;
        return new Match(start, end, replacement, tokens);
    }
}
