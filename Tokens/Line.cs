using System;
using System.Collections.Generic;

public class Line : TreeToken {
    public Line(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken Copy_(List<IToken> tokens) {
        return (TreeToken)new Line(tokens);
    }
}
