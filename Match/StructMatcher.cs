using System;
using System.Collections.Generic;

public class StructMatcher : IMatcher {
    public Match Match(IToken tokens_) {
        if (!(tokens_ is TreeToken)) return null;
        TreeToken tokens = (TreeToken)tokens_;
        int i = 0;
        foreach (IToken token in tokens) {
            
            i++;
        }
        return null;
    }
}
