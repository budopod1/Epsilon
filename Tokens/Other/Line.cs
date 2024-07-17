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
                "Invalid syntax; Are you missing a semicolon?", this[1]
            );
        }
        if (Count == 0) return;
        IToken token = this[0];
        if (!(token is ICompleteLine)) {
            throw new SyntaxErrorException(
                "Incomplete line", token
            );
        }
        if (token is IBlockEndOnly) {
            CodeBlock block = (CodeBlock)parent;
            int index = block.GetTokens().FindIndex(line=>line==this);
            if (index != block.Count-1) {
                throw new SyntaxErrorException(
                    $"{token.GetType().Name} is only valid at the end of a block", token
                );
            }
        }
    }
}
