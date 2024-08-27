public class AndPatternSegment : IPatternSegment {
    readonly List<IPatternSegment> segments;

    public List<IPatternSegment> GetSegments() {
        return segments;
    }

    public AndPatternSegment(List<IPatternSegment> segments) {
        this.segments = segments;
    }

    public AndPatternSegment(IPatternSegment a, IPatternSegment b) {
        segments = [a, b];
    }

    public bool Matches(IToken token) {
        foreach (IPatternSegment segment in segments) {
            if (!segment.Matches(token)) return false;
        }
        return true;
    }

    public bool Equals(IPatternSegment obj) {
        AndPatternSegment other = obj as AndPatternSegment;
        if (other == null) return false;
        return Enumerable.SequenceEqual<IPatternSegment>(segments, other.GetSegments());
    }
}
