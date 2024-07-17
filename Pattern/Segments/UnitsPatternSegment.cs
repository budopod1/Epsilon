using System;
using System.Linq;
using System.Collections.Generic;

public class UnitsPatternSegment<T> : IPatternSegment where T : IEquatable<T> {
    List<T> values;
    Type unit;

    public List<T> GetValue() {
        return values;
    }

    public UnitsPatternSegment(Type unit, List<T> values) {
        this.values = values;
        this.unit = unit;
    }

    public bool Matches(IToken token) {
        if (!(token is Unit<T> && Utils.IsInstance(token, unit))) return false;
        T target = ((Unit<T>)token).GetValue();
        foreach (T value in values) {
            if (value.Equals(target)) return true;
        }
        return false;
    }

    public bool Equals(IPatternSegment obj) {
        UnitsPatternSegment<T> other = obj as UnitsPatternSegment<T>;
        if (other == null) return false;
        return values.SequenceEqual(other.GetValue());
    }
}
