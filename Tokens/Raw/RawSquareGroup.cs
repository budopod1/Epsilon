using System;
using System.Collections.Generic;

public class RawSquareGroup : TreeToken {
    public RawSquareGroup(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawSquareGroup(tokens);
    }
}
