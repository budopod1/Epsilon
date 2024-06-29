using System;
using System.Linq;
using System.Collections.Generic;

public class RawFor : IParentToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public static List<string> ClauseNames = new List<string> {
        "to", "from", "step", "in", "enumerating"
    };

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
                block = ((CodeBlock)value);
            } else {
                clauses[i-1] = ((RawForClause)value);
            }
        }
    }
    
    List<RawForClause> clauses;
    int declarationID;
    Type_ type_;
    CodeBlock block;
    
    public RawFor(List<IToken> condition, CodeBlock block) {
        VarDeclaration declaration = condition[0] as VarDeclaration;
        if (declaration == null) {
            throw new SyntaxErrorException(
                "For loop condition must start with variable declaration", condition[0]
            );
        }
        string declarationName = declaration.GetName().GetValue();
        type_ = declaration.GetType_();
        declarationID = block.GetScope().AddVar(
            declarationName, type_
        );
        Name startingClauseToken = condition[1] as Name;
        if (startingClauseToken == null) {
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
        RawForClause clause = new RawForClause(startingClauseName);
        clauses = new List<RawForClause> {clause};
        foreach (IToken token in condition.Skip(2)) {
            Name name = token as Name;
            if (name != null) {
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
