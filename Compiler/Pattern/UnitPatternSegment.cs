using System;

public class UnitPatternSegment<T> : IPatternSegment where T : IEquatable<T> {
    T value;
    Type unit;

    public T GetValue() {
        return value;
    }
    
    public UnitPatternSegment(Type unit, T value) {
        this.value = value;
        this.unit = unit;
    }

    public bool Matches(IToken token) {
        return (token is Unit<T> && Utils.IsInstance(token, unit)
            && ((Unit<T>)token).GetValue().Equals(value));
    }

    public bool Equals(IPatternSegment obj) {
        UnitPatternSegment<T> other = obj as UnitPatternSegment<T>;
        if (other == null) return false;
        return value.Equals(other.GetValue());
    }
}
