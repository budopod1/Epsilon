using System;
using System.Collections.Generic;

public class RawSquareGroup(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawSquareGroup(tokens);
    }
}
