using System;
using System.Collections.Generic;

public class Generics : TreeToken {
    public Generics(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken Copy_(List<IToken> tokens) {
        return (TreeToken)new Generics(tokens);
    }
}
