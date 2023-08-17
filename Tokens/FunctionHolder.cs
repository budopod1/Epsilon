using System;
using System.Collections.Generic;

public class FunctionHolder : Holder {
    public FunctionHolder(List<Token> tokens) : base(tokens) {}
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new FunctionHolder(tokens);
    }

    public RawFuncTemplate GetRawTemplate() {
        if (this.Count < 2) return null;
        Token token = this[0];
        if (!(token is RawFuncTemplate)) return null;
        return (RawFuncTemplate)token;
    }

    public FuncTemplate GetTemplate() {
        if (this.Count < 2) return null;
        Token token = this[0];
        if (!(token is FuncTemplate)) return null;
        return (FuncTemplate)token;
    }

    public void SetTemplate(Token template) {
        if (this.Count < 2)
            throw new InvalidOperationException(
                "FuncHolder does not have template already set"
            );
        this[0] = template;
    }
}
