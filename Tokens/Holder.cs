using System;
using System.Collections.Generic;

public class Holder : TreeToken {
    public Holder(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new Holder(tokens);
    }

    public Block GetBlock() {
        if (this.Count < 2) return null;
        IToken token = this[1];
        if (!(token is Block)) return null;
        return (Block)token;
    }

    public void SetBlock(Block block) {
        if (this.Count < 2)
            throw new InvalidOperationException("Holder does not have block already set");
        this[1] = block;
    }
}
