using System;
using System.Collections.Generic;

public class RawGlobal : TreeToken {
    public RawGlobal(List<IToken> tokens) : base(tokens) {}

    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawGlobal(tokens);
    }
}
