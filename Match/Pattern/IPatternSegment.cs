using System;

public interface IPatternSegment {
    bool Matches(IToken token);
    void Reset();
}
