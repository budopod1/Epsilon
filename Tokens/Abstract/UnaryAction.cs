public abstract class UnaryAction<T>(T o) : IParentToken where T : IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    protected T o = o;

    public int Count {
        get { return 1; }
    }

    public IToken this[int i] {
        get {
            return o;
        }
        set {
            o = (T)value;
        }
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, o.ToString());
    }

    public T Sub() {
        return o;
    }

    public void SetSub(T o) {
        this.o = o;
    }
}
