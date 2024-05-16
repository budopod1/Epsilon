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

    public bool DoesTerminateFunction() {
        if (Count == 0) return false;
        Line line = this[Count - 1] as Line;
        if (line == null) return false;
        line.Verify(); // make sure line.Count == 1
        IFunctionTerminator terminator = line[0] as IFunctionTerminator;
        if (terminator == null) return false;
        return terminator.DoesTerminateFunction();
    }
}
