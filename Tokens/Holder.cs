using System;
using System.Collections.Generic;

public class Holder : TreeToken {
    public Holder(List<Token> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new Holder(tokens);
    }

    public Block GetBlock() {
        if (this.Count < 2) return null;
        Token token = this[1];
        if (!(token is Block)) return null;
        return (Block)token;
    }

    public void SetBlock(Block block) {
        if (this.Count < 2)
            throw new InvalidOperationException("Holder does not have block already set");
        this[1] = block;
    }
}
