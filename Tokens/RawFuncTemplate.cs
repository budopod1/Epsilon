using System;
using System.Collections.Generic;

public class RawFuncTemplate : TreeToken {
    public RawFuncTemplate(List<Token> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new RawFuncTemplate(tokens);
    }
}
