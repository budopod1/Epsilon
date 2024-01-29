using System;
using System.Collections.Generic;

public class CodeBlock : Block {
    Program program;
    Scope scope;
    
    public CodeBlock(Program program, List<IToken> tokens) : base(tokens) {
        scope = new Scope(program, this);
        this.program = program;
    }
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        CodeBlock newBlock = new CodeBlock(program, tokens);
        newBlock.GetScope().CopyFrom(scope);
        return newBlock;
    }

    public Scope GetScope() {
        return scope;
    }
}
