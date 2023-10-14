using System;

public abstract class UnaryOperation<T> : IParentToken where T : IToken {
    public IParentToken parent { get; set; }
    
    protected T o;
    
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
    
    public UnaryOperation(T o) {
        this.o = o;
    }

    public override string ToString() {
        return Utils.WrapName(this.GetType().Name, o.ToString());
    }
}
