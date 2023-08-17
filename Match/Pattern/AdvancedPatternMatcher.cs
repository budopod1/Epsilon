using System;
using System.Collections.Generic;

public class AdvancedPatternMatcher : AdvancedPatternExtractor<Match>, IMatcher {
    public AdvancedPatternMatcher(
        List<IPatternSegment> start, List<IPatternSegment> repeated, int minRepeats,
        int maxRepeats, List<IPatternSegment> end,
        IPatternProcessor<List<Token>> subprocessor) {
        this.start = start;
        this.repeated = repeated;
        this.minRepeats = minRepeats;
        this.maxRepeats = maxRepeats;
        this.end = end;
        this.processor = new MatcherPatternProcessor(subprocessor);
    }

    public Match Match(ParentToken tokens) {
        return this.Extract(tokens);
    }
}
