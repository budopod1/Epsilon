using System;
using System.Collections.Generic;

public class FunctionHolder : Block {
    public FunctionHolder(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new FunctionHolder(tokens);
    }
}
