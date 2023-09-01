using System;
using System.Collections.Generic;

public class Symbolic : IToken {
    public IParentToken parent { get; set; }
    
    public override string ToString() {
        return "(" + this.GetType().Name + ")";
    }
}
