using System;
using System.Collections.Generic;

public abstract class Block : TreeToken {
    public Block(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new Block(tokens);
    }
}
