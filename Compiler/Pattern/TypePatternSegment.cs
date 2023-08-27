using System;

public class TypePatternSegment : IPatternSegment {
    Type type;

    public Type GetMType() {
        return type;
    }
    
    public TypePatternSegment(Type type) {
        this.type = type;
    }

    public bool Matches(IToken token) {
        return Utils.IsInstance(token, type);
    }

    public bool Equals(IPatternSegment obj) {
        TypePatternSegment other = obj as TypePatternSegment;
        if (other == null) return false;
        return type == other.GetMType();
    }
}
