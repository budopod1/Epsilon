using System;
using System.Collections.Generic;

public class RawGroup : TreeToken {
    public RawGroup(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken Copy_(List<IToken> tokens) {
        return (TreeToken)new RawGroup(tokens);
    }
}
