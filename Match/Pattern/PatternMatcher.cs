using System;
using System.Collections.Generic;

public class PatternMatcher : PatternExtractor<Match>, IMatcher {
    public PatternMatcher(List<IPatternSegment> segments, 
                          IPatternProcessor<List<IToken>> subprocessor) {
        this.segments = segments;
        this.processor = new MatcherPatternProcessor(subprocessor);
    }

    public Match Match(IParentToken tokens) {
        return this.Extract(tokens);
    }
}
