using System;
using System.Collections.Generic;

public class SplitTokensParser {
    Type seperator;
    bool allowUnterminated;
    
    public SplitTokensParser(Type seperator, bool allowUnterminated) {
        this.seperator = seperator;
        this.allowUnterminated = allowUnterminated;
    }

    public List<List<IToken>> Parse(IParentToken tree) {
        List<List<IToken>> result = new List<List<IToken>>();
        List<IToken> soFar = new List<IToken>();
        for (int i = 0; i < tree.Count; i++) {
            IToken token = tree[i];
            if (Utils.IsInstance(token, seperator)) {
                result.Add(soFar);
                soFar = new List<IToken>();
            } else {
                soFar.Add(token);
            }
        }
        if (allowUnterminated) {
            result.Add(soFar);
        } else if (soFar.Count > 0) {
            throw new ArgumentException("Unterimated tokens");
        } 
        return result;
    }
}
