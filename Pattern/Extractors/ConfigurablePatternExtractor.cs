namespace Epsilon;
public class ConfigurablePatternExtractor<T> : PatternExtractor<T> {
    public ConfigurablePatternExtractor(List<IPatternSegment> segments,
                                        IPatternProcessor<T> processor) {
        this.segments = segments;
        this.processor = processor;
    }

    public override string ToString() {
        IEnumerable<string> segmentsStrings = segments.Select(
            (IPatternSegment segment) => segment.ToString()
        );
        return Utils.WrapName(
            GetType().Name,
            string.Join(", ", segmentsStrings)
        );
    }
}
