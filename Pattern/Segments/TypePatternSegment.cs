namespace Epsilon;
public class TypePatternSegment : IPatternSegment {
    readonly Type type;
    readonly bool exact = false;

    public Type GetMType() {
        return type;
    }

    public TypePatternSegment(Type type) {
        this.type = type;
    }

    public TypePatternSegment(Type type, bool exact) {
        this.type = type;
        this.exact = exact;
    }

    public bool Matches(IToken token) {
        if (exact) {
            return token.GetType() == type;
        } else {
            return Utils.IsInstance(token, type);
        }
    }

    public bool Equals(IPatternSegment obj) {
        TypePatternSegment other = obj as TypePatternSegment;
        if (other == null) return false;
        return type == other.GetMType();
    }
}
