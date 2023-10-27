using System;

public class Break : IVerifier, ICompleteLine {
    public IParentToken parent { get; set; }

    public void Verify() {
        IToken parent = TokenUtils.GetParentWithCond(
            this, (IToken token) => (token is While)
        );
        if (parent == null) {
            throw new SyntaxErrorException(
                "Cannot break outside of a loop"
            );
        }
    }

    public override string ToString() {
        return this.GetType().Name + "()";
    }
}
