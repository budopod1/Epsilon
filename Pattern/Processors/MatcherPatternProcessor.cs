using System;
using System.Collections.Generic;

public class MatcherPatternProcessor(IPatternProcessor<List<IToken>> subprocessor) : IPatternProcessor<Match> {
    readonly IPatternProcessor<List<IToken>> subprocessor = subprocessor;

    public Match Process(List<IToken> tokens, int start, int end) {
        List<IToken> replacement = subprocessor.Process(tokens, start, end);
        if (replacement == null) return null;
        return new Match(start, end, replacement, tokens);
    }
}
