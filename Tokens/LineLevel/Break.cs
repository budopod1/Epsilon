using System;

public class Break : IVerifier, ICompleteLine, IBlockEndOnly {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public void Verify() {
        IToken parent = TokenUtils.GetParentWithCond(
            this, (IToken token) => (token is ILoop)
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

    public int Serialize(SerializationContext context) {
        ILoop loop = (ILoop)TokenUtils.GetParentWithCond(
            this, (IToken token) => (token is ILoop)
        );
        int? id = context.GetFunction().GetContextIdByBlock(loop.GetBlock());
        return context.AddInstruction(
            new SerializableInstruction(this).AddData("block", new JSONInt(id))
        );
    }
}
