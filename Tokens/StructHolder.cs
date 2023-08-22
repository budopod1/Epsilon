using System;
using System.Collections.Generic;

public class StructHolder : Holder {
    public StructHolder(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken Copy_(List<IToken> tokens) {
        return (TreeToken)new StructHolder(tokens);
    }
}
