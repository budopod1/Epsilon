using System;

public class ConditionPatternSegment<T> : IPatternSegment {
    Func<T, bool> cond;
    
    public ConditionPatternSegment(Func<T, bool> cond) {
        this.cond = cond;
    }

    public bool Matches(IToken token) {
        if (token is T) return cond((T)token);
        return false;
    }
}
