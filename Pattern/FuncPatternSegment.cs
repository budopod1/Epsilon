using System;
using System.Collections.Generic;

public class FuncPatternSegment<T> : IPatternSegment where T : class {
    Func<T, bool> func;

    public Func<T, bool> GetFunction() {
        return func;
    }

    public FuncPatternSegment(Func<T, bool> func) {
        this.func = func;
    }

    public bool Matches(IToken token) {
        T TToken = token as T;
        if (TToken == null) return false;
        return func(TToken);
    }

    public bool Equals(IPatternSegment obj) {
        FuncPatternSegment<T> other = obj as FuncPatternSegment<T>;
        if (other == null) return false;
        return func == other.GetFunction();
    }
}
