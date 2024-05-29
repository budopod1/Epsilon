using System;
using System.Reflection;
using System.Collections.Generic;

public class UnwrapperPatternProcessor : IPatternProcessor<List<IToken>> {
    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        List<IToken> result = new List<IToken>();
        foreach (IToken token in tokens) {
            if (token is TreeToken) {
                result.AddRange((TreeToken)token);
            } else if (token is IParentToken) {
                IParentToken parent = ((IParentToken)token);
                for (int i = 0; i < parent.Count; i++) {
                    result.Add(parent[i]);
                }
            }
        }
        return result;
    }
}
