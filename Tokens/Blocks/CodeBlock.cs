using System;
using System.Collections.Generic;

public class CodeBlock : Block, IHasScope {
    IDCounter scopeVarIDCounter;
    Scope scope;
    
    public CodeBlock(Program program, List<IToken> tokens) : base(tokens) {
        scopeVarIDCounter = program.GetScopeVarIDCounter();
        scope = new Scope(scopeVarIDCounter, this);
    }

    public CodeBlock(IDCounter scopeVarIDCounter, List<IToken> tokens) : base(tokens) {
        this.scopeVarIDCounter = scopeVarIDCounter;
        scope = new Scope(scopeVarIDCounter, this);
    }
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        CodeBlock newBlock = new CodeBlock(scopeVarIDCounter, tokens);
        newBlock.GetScope().CopyFrom(scope);
        return newBlock;
    }

    public IScope GetScope() {
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
