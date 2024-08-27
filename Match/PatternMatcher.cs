public class PatternMatcher : PatternExtractor<Match>, IMatcher {
    public PatternMatcher(List<IPatternSegment> segments,
                          IPatternProcessor<List<IToken>> subprocessor) {
        this.segments = segments;
        processor = new MatcherPatternProcessor(subprocessor);
    }

    public Match Match(IParentToken tokens) {
        return Extract(tokens);
    }
}
