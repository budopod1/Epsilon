using System;
using System.Collections.Generic;

public class PatternMatcher : PatternExtractor<Match>, IMatcher {
    public PatternMatcher(List<IPatternSegment> segments, 
                          IPatternProcessor<List<Token>> subprocessor) {
        this.segments = segments;
        this.processor = new MatcherPatternProcessor(subprocessor);
    }

    public Match Match(ParentToken tokens) {
        return this.Extract(tokens);
    }
}
