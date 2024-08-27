public class FuncArgPatternSegment : IPatternSegment {
    public bool Matches(IToken token) {
        return token is RawSquareGroup;
    }

    public bool Equals(IPatternSegment other) {
        return other is FuncArgPatternSegment;
    }
}
