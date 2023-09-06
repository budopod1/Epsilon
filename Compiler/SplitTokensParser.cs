using System;
using System.Collections.Generic;

public class SplitTokensParser {
    IPatternSegment seperator;
    bool allowUnterminated;
    
    public SplitTokensParser(IPatternSegment seperator, bool allowUnterminated) {
        this.seperator = seperator;
        this.allowUnterminated = allowUnterminated;
    }

    public List<List<IToken>> Parse(IParentToken tree) {
        List<List<IToken>> result = new List<List<IToken>>();
        List<IToken> soFar = new List<IToken>();
        for (int i = 0; i < tree.Count; i++) {
            IToken token = tree[i];
            if (seperator.Matches(token)) {
                result.Add(soFar);
                soFar = new List<IToken>();
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
