using System;

public class TypePatternSegment : IPatternSegment {
    Type type;
    
    public TypePatternSegment(Type type) {
        this.type = type;
    }

    public bool Matches(Token token) {
        return Utils.IsInstance(token, type);
    }
}
