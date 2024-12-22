namespace Epsilon;
public class AbortVoid : IFunctionTerminator, IBlockEndOnly, ICanAbort {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public override string ToString() {
        return GetType().Name + "()";
    }

    public int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this).Register();
    }

    public bool DoesTerminateFunction() {
        return true;
    }

    public bool CanAbort() {
        return true;
    }
}
