using System;
using System.Collections.Generic;

public class RawFunctionArgument : TreeToken {
    public RawFunctionArgument(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new RawFunctionArgument(tokens);
    }
}
