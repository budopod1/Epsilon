namespace Epsilon;
public class ConfigurableSurroundedPatternExtractor<T> : SurroundedPatternExtractor<T> {
    public ConfigurableSurroundedPatternExtractor(IPatternSegment start, IPatternSegment end) {
        this.start = start;
        this.end = end;
    }
}
