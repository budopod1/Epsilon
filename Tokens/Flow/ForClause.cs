public class ForClause : UnaryOperation<IValueToken> {
    readonly string name;

    public ForClause(RawForClause source) : base(null) {
        span = source.span;
        name = source.GetName();
        List<IToken> tokens = source.GetTokens();
        if (tokens.Count == 0) {
            throw new SyntaxErrorException(
                "Empty for clause", source
            );
        }
        if (tokens.Count > 1) {
            throw new SyntaxErrorException(
                "Malformed for clause", tokens[1]
            );
        }
        if (tokens[0] is not IValueToken value) {
            throw new SyntaxErrorException(
                "For clause must contain a value", tokens[0]
            );
        }
        SetSub(value);
    }

    public ForClause(string name, IValueToken o) : base(o) {
        this.name = name;
    }

    public IValueToken GetValue() {
        return o;
    }

    public string GetName() {
        return name;
    }
}
