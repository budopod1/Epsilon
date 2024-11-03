namespace Epsilon;
public abstract class PatternExtractor<T> : ITokenExtractor<T>, IEquatable<PatternExtractor<T>> {
    protected List<IPatternSegment> segments;
    protected IPatternProcessor<T> processor;

    Action<List<IToken>, int, int> callback;

    public void SetCallback(Action<List<IToken>, int, int> callback) {
        this.callback = callback;
    }

    public List<IPatternSegment> GetSegments() {
        return segments;
    }

    public T Extract(IParentToken tokens) {
        int maxStart = tokens.Count - segments.Count;
        for (int i = 0; i <= maxStart; i++) {
            bool matches = true;
            int j;
            List<IToken> tokenList = [];
            for (j = 0; j < segments.Count; j++) {
                IPatternSegment segment = segments[j];
                IToken token = tokens[i+j];
                tokenList.Add(token);
                if (!segment.Matches(token)) {
                    matches = false;
                    break;
                }
            }
            if (matches) {
                if (callback != null) {
                    callback(tokenList, i, i+j-1);
                    callback = null;
                }
                return processor.Process(tokenList, i, i+j-1);
            }
        }
        callback = null;
        return default;
    }

    public bool Equals(PatternExtractor<T> other) {
        return Enumerable.SequenceEqual(segments, other.GetSegments());
    }
}
