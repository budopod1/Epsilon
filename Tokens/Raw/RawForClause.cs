public class RawForClause : TreeToken {
    readonly string name;

    public RawForClause(string name) : base([]) {
        this.name = name;
    }

    public RawForClause(string name, List<IToken> tokens) : base(tokens) {
        this.name = name;
    }

    protected override TreeToken _Copy(List<IToken> tokens) {
        return new RawForClause(name, tokens);
    }

    public string GetName() {
        return name;
    }
}
