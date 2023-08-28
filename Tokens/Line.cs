using System;
using System.Collections.Generic;

public class Line : TreeToken {
    public Line(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new Line(tokens);
    }
}
