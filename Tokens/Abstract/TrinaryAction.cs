namespace Epsilon;
public abstract class TrinaryAction<T1, T2, T3>(T1 o1, T2 o2, T3 o3) : IParentToken where T1 : IToken where T2 : IToken where T3 : IToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    protected T1 o1 = o1;
    protected T2 o2 = o2;
    protected T3 o3 = o3;

    public int Count {
        get { return 3; }
    }

    public IToken this[int i] {
        get {
            if (i == 0) return o1;
            if (i == 1) return o2;
            return o3;
        }
        set {
            if (i == 0) {
                o1 = (T1)value;
            } else if (i == 1) {
                o2 = (T2)value;
            } else {
                o3 = (T3)value;
            }
        }
    }

    public override string ToString() {
        return Utils.WrapName(
            GetType().Name, $"{o1}, {o2}, {o3}"
        );
    }
}
