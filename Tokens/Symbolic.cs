using System;
using System.Collections.Generic;

public class Symbolic : Token {
    public override string ToString() {
        return "(" + this.GetType().Name + ")";
    }
}
