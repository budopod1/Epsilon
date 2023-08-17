using System;

public class UnitPatternSegment<T> : IPatternSegment where T : IEquatable<T> {
    T value;
    Type unit;
    
    public UnitPatternSegment(Type unit, T value) {
        this.value = value;
        this.unit = unit;
    }

    public bool Matches(Token token) {
        return (token is Unit<T> && Utils.IsInstance(token, unit)
            && ((Unit<T>)token).GetValue().Equals(value));
    }
}
