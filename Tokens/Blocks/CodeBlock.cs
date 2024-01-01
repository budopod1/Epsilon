using System;
using System.Collections.Generic;

public class CodeBlock : Block {
    Scope scope;
    
    public CodeBlock(List<IToken> tokens) : base(tokens) {
        scope = new Scope(this);
    }
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        CodeBlock newBlock = new CodeBlock(tokens);
        newBlock.GetScope().CopyFrom(scope);
        return newBlock;
    }

    public Scope GetScope() {
        return scope;
    }
}
