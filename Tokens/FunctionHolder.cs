using System;
using System.Collections.Generic;

public class FunctionHolder : Holder {
    public FunctionHolder(List<IToken> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new FunctionHolder(tokens);
    }

    public RawFuncTemplate GetRawTemplate() {
        if (this.Count < 2) return null;
        IToken token = this[0];
        if (!(token is RawFuncTemplate)) return null;
        return (RawFuncTemplate)token;
    }

    public void SetTemplate(IToken template) {
        if (this.Count < 2)
            throw new InvalidOperationException(
                "FuncHolder does not have template already set"
            );
        this[0] = template;
    }
}
