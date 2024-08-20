using System;
using System.Linq;
using System.Collections.Generic;

public class TypesPatternSegment(List<Type> types) : IPatternSegment {
    readonly List<Type> types = types;

    public List<Type> GetMTypes() {
        return types;
    }

    public bool Matches(IToken token) {
        foreach (Type type in types) {
            if (Utils.IsInstance(token, type)) {
                return true;
            }
        }
        return false;
    }

    public bool Equals(IPatternSegment obj) {
        TypesPatternSegment other = obj as TypesPatternSegment;
        if (other == null) return false;
        return types.SequenceEqual(other.GetMTypes());
    }
}
