using System;
using System.Collections.Generic;

public class Symbolic : IToken {
    public override string ToString() {
        return this.GetType().Name;
    }
}
