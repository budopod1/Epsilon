
namespace Epsilon;
public class SurroundedPatternMatcher : SurroundedPatternExtractor<Match>, IMatcher {
    public SurroundedPatternMatcher(IPatternSegment start, IPatternSegment end, IPatternProcessor<List<IToken>> subprocessor) {
        this.start = start;
        this.end = end;
        processor = new MatcherPatternProcessor(subprocessor);
    }

    public Match Match(IParentToken tokens) {
        return Extract(tokens);
    }
}
