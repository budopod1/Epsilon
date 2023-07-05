using System;
using System.Collections.Generic;

public class StructHolder : Block {
    public StructHolder(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new StructHolder(tokens);
    }
}
