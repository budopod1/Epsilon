public class CodeBlock : Block, IHasScope {
    readonly Scope scope;

    public CodeBlock(Program program, List<IToken> tokens) : base(tokens) {
        scope = new Scope(program.GetScopeVarIDCounter());
    }

    public CodeBlock(Scope scope, List<IToken> tokens) : base(tokens) {
        this.scope = scope;
    }

    protected override TreeToken _Copy(List<IToken> tokens) {
        return new CodeBlock(scope, tokens);
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
