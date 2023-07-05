using System;
using System.Collections.Generic;

public class Block : TreeToken {
    public Block(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Block(tokens);
    }
}
