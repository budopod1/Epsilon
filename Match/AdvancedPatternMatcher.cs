public class AdvancedPatternMatcher : AdvancedPatternExtractor<Match>, IMatcher {
    public AdvancedPatternMatcher(
        List<IPatternSegment> start, List<IPatternSegment> repeated, int minRepeats,
        int maxRepeats, List<IPatternSegment> end,
        IPatternProcessor<List<IToken>> subprocessor) {
        this.start = start;
        this.repeated = repeated;
        this.minRepeats = minRepeats;
        this.maxRepeats = maxRepeats;
        this.end = end;
        processor = new MatcherPatternProcessor(subprocessor);
    }

    public Match Match(IParentToken tokens) {
        return Extract(tokens);
    }
}
