public class RawFor : IParentToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public static List<string> ClauseNames = [
        "to", "from", "step", "in", "enumerating"
    ];

    public int Count {
        get => 1 + clauses.Count;
    }

    public IToken this[int i] {
        get {
            if (i == 0) {
                return block;
            } else {
                return clauses[i-1];
            }
        }
        set {
            if (i == 0) {
                block = (CodeBlock)value;
            } else {
                clauses[i-1] = (RawForClause)value;
            }
        }
    }

    readonly List<RawForClause> clauses;
    readonly int declarationID;
    readonly Type_ type_;
    CodeBlock block;

    public RawFor(List<IToken> condition, CodeBlock block) {
        if (condition[0] is not VarDeclaration declaration) {
            throw new SyntaxErrorException(
                "For loop condition must start with variable declaration", condition[0]
            );
        }
        string declarationName = declaration.GetName().GetValue();
        type_ = declaration.GetType_();
        declarationID = block.GetScope().AddVar(
            declarationName, type_
        );
        if (condition[1] is not Name startingClauseToken) {
            throw new SyntaxErrorException(
                "For loop condition must start with a clause", condition[1]
            );
        }
        string startingClauseName = startingClauseToken.GetValue();
        if (!ClauseNames.Contains(startingClauseName)) {
            throw new SyntaxErrorException(
                "Invalid clause name", startingClauseToken
            );
        }
        RawForClause clause = new(startingClauseName);
        clauses = [clause];
        foreach (IToken token in condition.Skip(2)) {
            if (token is Name name){
                string clauseName = name.GetValue();
                if (ClauseNames.Contains(clauseName)) {
                    clause = new RawForClause(clauseName);
                    clauses.Add(clause);
                    continue;
                }
            }
            clause.Add(token);
        }
        this.block = block;
    }

    public List<RawForClause> GetClauses() {
        return clauses;
    }

    public int GetDeclarationID() {
        return declarationID;
    }

    public Type_ GetType_() {
        return type_;
    }

    public CodeBlock GetBlock() {
        return block;
    }
}
