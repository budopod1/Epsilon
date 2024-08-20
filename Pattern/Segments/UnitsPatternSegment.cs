using System;
using System.Linq;
using System.Collections.Generic;

public class UnitsPatternSegment<T>(Type unit, List<T> values) : IPatternSegment where T : IEquatable<T> {
    readonly List<T> values = values;
    readonly Type unit = unit;

    public List<T> GetValue() {
        return values;
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
