using System;
using System.Collections.Generic;

public class FunctionHolder : Holder {
    public FunctionHolder(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new FunctionHolder(tokens);
    }

    public RawFuncSignature GetRawSignature() {
        if (this.Count < 2) return null;
        IToken token = this[0];
        if (!(token is RawFuncSignature)) return null;
        return (RawFuncSignature)token;
    }

    public FuncSignature GetSignature() {
        if (this.Count < 2) return null;
        IToken token = this[0];
        if (!(token is FuncSignature)) return null;
        return (FuncSignature)token;
    }

    public void SetSignature(IToken signature) {
        if (this.Count < 2)
            throw new InvalidOperationException(
                "FunctionHolder does not have signature already set"
            );
        this[0] = signature;
    }
}
