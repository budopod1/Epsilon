using System;

public interface IPatternSegment {
    bool Matches(IToken token);
}
