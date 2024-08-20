using System;
using System.Collections.Generic;

public class SplitTokensParser(IPatternSegment seperator, bool allowUnterminated) {
    readonly IPatternSegment seperator = seperator;
    readonly bool allowUnterminated = allowUnterminated;

    public List<List<IToken>> Parse(IParentToken tree) {
        List<IToken> list = [];
        for (int i = 0; i < tree.Count; i++) {
            list.Add(tree[i]);
        }
        return Parse(list);
    }

    public List<List<IToken>> Parse(List<IToken> tree) {
        List<List<IToken>> result = [];
        List<IToken> soFar = [];
        for (int i = 0; i < tree.Count; i++) {
            IToken token = tree[i];
            if (seperator.Matches(token)) {
                result.Add(soFar);
                soFar = [];
            } else {
                soFar.Add(token);
            }
        }
        if (allowUnterminated) {
            result.Add(soFar);
        } else if (soFar.Count > 0) {
            return null;
        }
        return result;
    }
}
