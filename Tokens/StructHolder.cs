using System;
using System.Collections.Generic;

public class StructHolder : Holder {
    public StructHolder(List<Token> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new StructHolder(tokens);
    }
}
