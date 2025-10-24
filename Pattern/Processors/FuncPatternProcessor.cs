namespace Epsilon;
public class FuncPatternProcessor<T> : IPatternProcessor<T> {
    readonly Func<List<IToken>, int, int, T> func;

    public FuncPatternProcessor(Func<List<IToken>, int, int, T> func) {
        this.func = func;
    }

    public FuncPatternProcessor(Func<List<IToken>, T> func) {
        this.func = (token, start, end) => func(token);
    }

    public T Process(List<IToken> tokens, int start, int end) {
        return func(tokens, start, end);
    }
}
