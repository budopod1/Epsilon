namespace Epsilon;
public interface IPatternSegment : IEquatable<IPatternSegment> {
    bool Matches(IToken token);
}
