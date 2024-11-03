namespace Epsilon;
public class UnwrapperPatternProcessor : IPatternProcessor<List<IToken>> {
    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        List<IToken> result = [];
        foreach (IToken token in tokens) {
            if (token is TreeToken token1) {
                result.AddRange(token1);
            } else if (token is IParentToken parent) {
                for (int i = 0; i < parent.Count; i++) {
                    result.Add(parent[i]);
                }
            }
        }
        return result;
    }
}
