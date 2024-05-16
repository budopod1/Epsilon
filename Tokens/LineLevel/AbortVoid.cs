using System;

public class AbortVoid : IFunctionTerminator, IBlockEndOnly {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public override string ToString() {
        return GetType().Name + "()";
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(new SerializableInstruction(this));
    }

    public bool DoesTerminateFunction() {
        return true;
    }
}
