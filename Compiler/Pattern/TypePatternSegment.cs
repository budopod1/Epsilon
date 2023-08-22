using System;

public class TypePatternSegment : IPatternSegment {
    Type type;
    
    public TypePatternSegment(Type type) {
        this.type = type;
    }

    public bool Matches(IToken token) {
        return Utils.IsInstance(token, type);
    }
}
