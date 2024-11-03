namespace Epsilon;
public class FuncPatternSegment<T>(Func<T, bool> func) : IPatternSegment where T : class {
    readonly Func<T, bool> func = func;

    public Func<T, bool> GetFunction() {
        return func;
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
