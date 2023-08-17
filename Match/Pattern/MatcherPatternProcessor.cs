using System;
using System.Collections.Generic;

public class MatcherPatternProcessor : IPatternProcessor<Match> {
    IPatternProcessor<List<Token>> subprocessor;

    public MatcherPatternProcessor(IPatternProcessor<List<Token>> subprocessor) {
        this.subprocessor = subprocessor;
    }
    
    public Match Process(List<Token> tokens, int start, int end) {
        return new Match(start, end, subprocessor.Process(tokens, start, end), tokens);
    }
}
