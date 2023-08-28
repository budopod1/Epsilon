using System;
using System.Collections.Generic;

public class RawParameterGroup : TreeToken {
    public RawParameterGroup(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawParameterGroup(tokens);
    }
}
