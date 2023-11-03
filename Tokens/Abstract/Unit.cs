using System;

public abstract class Unit<T> : IToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    T value;
    
    public Unit(T value) {
        this.value = value;
    }

    public T GetValue() {
        return value;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, value.ToString());
    }
}
