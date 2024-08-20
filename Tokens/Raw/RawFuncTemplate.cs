using System;
using System.Collections.Generic;

public class RawFuncTemplate(List<IToken> tokens) : TreeToken(tokens) {
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawFuncTemplate(tokens);
    }
}
