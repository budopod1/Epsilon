using System;
using System.Collections.Generic;

public class Block : TreeToken {
    public Block(List<Token> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new Block(tokens);
    }
}
