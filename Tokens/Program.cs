using System;
using System.Collections.Generic;

public class Program : TreeToken { 
    Constants constants;
    List<string> baseTypes_ = null;
    
    public Program(List<IToken> tokens,
                    Constants constants) : base(tokens) {
        this.constants = constants;
    }
    
    public Program(List<IToken> tokens, Constants constants,
                   List<string> baseTypes_) : base(tokens) {
        this.constants = constants;
        this.baseTypes_ = baseTypes_;
    }

    public Constants GetConstants() {
        return constants;
    }

    public List<string> GetBaseTypes_() {
        return baseTypes_;
    }

    public void SetBaseTypes_(List<string> baseTypes_) {
        this.baseTypes_ = baseTypes_;
    }
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Program(tokens, constants, baseTypes_);
    }
}
