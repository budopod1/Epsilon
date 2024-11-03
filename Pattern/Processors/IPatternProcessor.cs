namespace Epsilon;
public interface IPatternProcessor<T> {
    T Process(List<IToken> tokens, int start, int end);
}
