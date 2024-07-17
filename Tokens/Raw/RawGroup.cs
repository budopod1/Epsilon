using System;
using System.Collections.Generic;

public class RawGroup : TreeToken {
    public RawGroup(List<IToken> tokens) : base(tokens) {}

    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawGroup(tokens);
    }
}
