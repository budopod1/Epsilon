using System;
using System.Collections.Generic;

public class CodeBlock : Block {
    public CodeBlock(List<IToken> tokens) : base(tokens) {}
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new CodeBlock(tokens);
    }
}
