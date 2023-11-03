using System;

public class Continue : IVerifier, ICompleteLine {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public void Verify() {
        IToken parent = TokenUtils.GetParentWithCond(
            this, (IToken token) => (token is While)
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
}
