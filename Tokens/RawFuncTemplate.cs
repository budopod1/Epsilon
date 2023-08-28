using System;
using System.Collections.Generic;

public class RawFuncTemplate : TreeToken, IBarMatchingInto {
    public RawFuncTemplate(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken Copy_(List<IToken> tokens) {
        return (TreeToken)new RawFuncTemplate(tokens);
    }
}
