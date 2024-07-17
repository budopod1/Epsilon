using System;
using System.Collections.Generic;

public abstract class Symbolic : IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public override string ToString() {
        return "(" + GetType().Name + ")";
    }

    public void Verify() {
        throw new SyntaxErrorException(
            "Unmatched "+GetType().Name, this
        );
    }
}
