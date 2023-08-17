using System;
using System.Collections.Generic;

public class RawFunctionArgument : TreeToken {
    public RawFunctionArgument(List<Token> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new RawFunctionArgument(tokens);
    }
}
