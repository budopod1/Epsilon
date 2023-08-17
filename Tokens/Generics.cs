using System;
using System.Collections.Generic;

public class Generics : TreeToken {
    public Generics(List<Token> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new Generics(tokens);
    }
}
