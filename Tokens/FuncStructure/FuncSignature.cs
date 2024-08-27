public class FuncSignature(Type_ returnType_, FuncTemplate template) : IParentToken, IBarMatchingInto {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly Type_ returnType_ = returnType_;
    FuncTemplate template = template;

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
