using System;
using System.Collections.Generic;

public class FuncSignature : IParentToken, IBarMatchingInto {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    Type_ returnType_;
    FuncTemplate template;

    public int Count {
        get { return 1; }
    }

    public IToken this[int i] {
        get {
            return template;
        }
        set {
            template = (FuncTemplate)value;
        }
    }

    public FuncSignature(Type_ returnType_, FuncTemplate template) {
        this.returnType_ = returnType_;
        this.template = template;
    }

    public Type_ GetReturnType_() {
        return returnType_;
    }

    public FuncTemplate GetTemplate() {
        return template;
    }

    public override string ToString() {
        return Utils.WrapName(
            GetType().Name, $"{returnType_.ToString()}, {template.ToString()}"
        );
    }
}
