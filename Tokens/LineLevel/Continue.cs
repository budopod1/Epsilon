public class Continue : IVerifier, IBlockEndOnly {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public void Verify() {
        IToken parent = TokenUtils.GetParentWithCond(
            this, (IToken token) => token is ILoop
        );
        if (parent == null) {
            throw new SyntaxErrorException(
                "Cannot continue outside of a loop", this
            );
        }
    }

    public override string ToString() {
        return GetType().Name + "()";
    }

    public int Serialize(SerializationContext context) {
        return new SerializableInstruction(context, this).Register();
    }
}
