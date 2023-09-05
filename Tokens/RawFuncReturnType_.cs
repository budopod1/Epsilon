using System;
using System.Collections.Generic;

public class RawFuncReturnType_ : TreeToken {
    public RawFuncReturnType_(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawFuncReturnType_(tokens);
    }

    public Type_ GetType_() {
        if (Count != 1) return Type_.Void();
        Type_Token token = this[0] as Type_Token;
        if (token == null) return Type_.Void();
        return token.GetValue();
    }
}
