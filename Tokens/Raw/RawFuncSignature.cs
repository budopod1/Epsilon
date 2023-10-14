using System;
using System.Collections.Generic;

public class RawFuncSignature : IParentToken, IBarMatchingInto {
    public IParentToken parent { get; set; }
    
    IToken returnType_;
    IToken template;
    
    public int Count {
        get { return 2; }
    }
    
    public IToken this[int i] {
        get {
            if (i == 0) return returnType_;
            return template;
        }
        set {
            if (i == 0) {
                returnType_ = value;
            } else {
                template = value;
            }
        }
    }
    
    public RawFuncSignature(IToken returnType_, IToken template) {
        this.returnType_ = returnType_;
        this.template = template;
    }

    public IToken GetReturnType_() {
        return returnType_;
    }

    public IToken GetTemplate() {
        return template;
    }

    public void SetReturnType_(IToken token) {
        returnType_ = token;
    }

    public void SetTemplate(IToken token) {
        template = token;
    }

    public override string ToString() {
        return Utils.WrapName(
            this.GetType().Name, $"{returnType_.ToString()}, {template.ToString()}"
        );
    }
}
