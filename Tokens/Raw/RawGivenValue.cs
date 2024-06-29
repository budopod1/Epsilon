using System;
using System.Collections.Generic;

public class RawGivenValue : TreeToken {
    public RawGivenValue(List<IToken> tokens) : base(tokens) {
        span = TokenUtils.MergeSpans(tokens);
    }

    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawGivenValue(tokens);
    }
}
