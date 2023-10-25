using System;
using System.Collections.Generic;

public class Line : TreeToken, IVerifier {
    public Line(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new Line(tokens);
    }

    public void Verify() {
        if (Count > 1) {
            throw new SyntaxErrorException(
                "Expected semicolon"
            );
        }
        IToken token = this[0];
        if (token is CodeBlock) {
            throw new SyntaxErrorException(
                "Unmatched block"
            );
        }
    }
}
