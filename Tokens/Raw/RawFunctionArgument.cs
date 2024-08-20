using System;
using System.Collections.Generic;

public class RawFunctionArgument(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawFunctionArgument(tokens);
    }
}
