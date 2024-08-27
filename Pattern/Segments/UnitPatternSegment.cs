public class UnitPatternSegment<T>(Type unit, T value) : IPatternSegment where T : IEquatable<T> {
    readonly T value = value;
    readonly Type unit = unit;

    public T GetValue() {
        return value;
    }

    public bool Matches(IToken token) {
        return
            token is Unit<T>
            && Utils.IsInstance(token, unit)
            && ((Unit<T>)token).GetValue().Equals(value)
        ;
    }

    public bool Equals(IPatternSegment obj) {
        UnitPatternSegment<T> other = obj as UnitPatternSegment<T>;
        if (other == null) return false;
        return value.Equals(other.GetValue());
    }
}
