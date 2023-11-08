using System;

public abstract class BinaryOperation<T1, T2> : IParentToken, ISerializableToken where T1 : IToken where T2 : IToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    protected T1 o1;
    protected T2 o2;
    
    public int Count {
        get { return 2; }
    }
    
    public IToken this[int i] {
        get {
            if (i == 0) return o1;
            return o2;
        }
        set {
            if (i == 0) {
                o1 = (T1)value;
            } else {
                o2 = (T2)value;
            }
        }
    }
    
    public BinaryOperation(T1 o1, T2 o2) {
        this.o1 = o1;
        this.o2 = o2;
    }

    public override string ToString() {
        return Utils.WrapName(
            GetType().Name, $"{o1.ToString()}, {o2.ToString()}"
        );
    }

    public virtual int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
        );
    }
}
