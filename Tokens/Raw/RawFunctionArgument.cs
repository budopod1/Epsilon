using System;
using System.Collections.Generic;

public class RawFunctionArgument : TreeToken {
    public RawFunctionArgument(List<IToken> tokens) : base(tokens) {}

    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawFunctionArgument(tokens);
    }
}
