using System;
using System.Collections.Generic;

public class Generics : TreeToken {
    public Generics(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Generics(tokens);
    }
}
