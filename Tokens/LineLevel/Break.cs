namespace Epsilon;
public class Break : IVerifier, ICompleteLine, IBlockEndOnly {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public void Verify() {
        IToken parent = TokenUtils.GetParentWithCond(
            this, token => token is ILoop
        );
        if (parent == null) {
            throw new SyntaxErrorException(
                "Cannot break outside of a loop", this
            );
        }
    }

    public override string ToString() {
        return GetType().Name + "()";
    }

    public int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this).Register();
    }
}
