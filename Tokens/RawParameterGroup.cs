using System;
using System.Collections.Generic;

public class RawParameterGroup : TreeToken {
    public RawParameterGroup(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken Copy_(List<IToken> tokens) {
        return (TreeToken)new RawParameterGroup(tokens);
    }
}
