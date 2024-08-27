public class RawFuncSignature(IToken returnType_, IToken template) : IParentToken, IBarMatchingInto {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    IToken returnType_ = returnType_;
    IToken template = template;

    public int Count {
        get => 2;
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
            GetType().Name, $"{returnType_.ToString()}, {template.ToString()}"
        );
    }
}
