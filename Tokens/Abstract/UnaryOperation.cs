using System;

public abstract class UnaryOperation<T> : IParentToken, ISerializableToken where T : IToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

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
        return Utils.WrapName(GetType().Name, o.ToString());
    }

    public T Sub() {
        return o;
    }

    public void SetSub(T o) {
        this.o = o;
    }

    public virtual int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
        );
    }
}
