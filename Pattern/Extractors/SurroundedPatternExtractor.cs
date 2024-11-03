namespace Epsilon;
public abstract class SurroundedPatternExtractor<T> : ITokenExtractor<T> {
    protected IPatternSegment start;
    protected IPatternSegment end;
    protected IPatternProcessor<T> processor;

    public T Extract(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken startToken = tokens[i];
            if (!start.Matches(startToken)) continue;
            List<IToken> tokenList = [];
            for (int j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                tokenList.Add(token);
                if (end.Matches(token)) {
                    return processor.Process(tokenList, i, j);
                }
            }
            return default;
        }
        return default;
    }
}
