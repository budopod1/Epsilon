using System;
using System.Collections.Generic;

public class RawFuncTemplate : TreeToken {
    public RawFuncTemplate(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawFuncTemplate(tokens);
    }
}
