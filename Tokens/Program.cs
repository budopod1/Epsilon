using System;
using System.Collections.Generic;

public class Program : Block { 
    Constants constants;
    
    public Program(List<IToken> tokens,
                    Constants constants) : base(tokens) {
        this.constants = constants;
    }

    public Constants GetConstants() {
        return constants;
    }
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Program(tokens, constants);
    }
}
