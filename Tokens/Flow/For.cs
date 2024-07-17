using System;
using System.Linq;
using System.Collections.Generic;

public class For : IParentToken, ILoop, IVerifier, ISerializableToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public int Count {
        get { return 1 + clauses.Count; }
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
                clauses[i-1] = ((ForClause)value);
            }
        }
    }

    List<ForClause> clauses;
    int declarationID;
    Type_ type_;
    CodeBlock block;

    public For(RawFor source) {
        clauses = source.GetClauses().Select(
            clause=>new ForClause(clause)
        ).ToList();
        declarationID = source.GetDeclarationID();
        type_ = source.GetType_();
        block = source.GetBlock();
    }

    public void Verify() {
        List<string> clauseNames = clauses.Select(
            clause=>clause.GetName()
        ).ToList();
        bool isInLoop = clauseNames.Contains("in");
        bool isEnumeratingLoop = clauseNames.Contains("enumerating");
        if (isInLoop && isEnumeratingLoop) {
            throw new SyntaxErrorException(
                "For loops cannot both have an in clause and an enumerating clause", this
            );
        }
        if (!isInLoop) {
            if (!type_.IsConvertibleTo(new Type_("Z"))) {
                throw new SyntaxErrorException(
                    "Non-in for loop variable type must be an integer", this
                );
            }
        }
        foreach (ForClause clause in clauses) {
            Type_ valType_ = clause.GetValue().GetType_();
            Type_ requiredType_ = Type_.Any();
            string clauseName = clause.GetName();
            switch (clauseName) {
            case "to":
                if (isEnumeratingLoop) {
                    throw new SyntaxErrorException(
                        "Enumerating for loops cannot have a to clause", clause
                    );
                }
                requiredType_ = isInLoop ? new Type_("W") : type_;
                break;
            case "from":
                requiredType_ = isInLoop ? new Type_("W") : type_;
                break;
            case "step":
                requiredType_ = new Type_("Z");
                break;
            case "in":
                requiredType_ = type_.ArrayOf();
                break;
            case "enumerating":
                requiredType_ = Type_.Any().ArrayOf();
                break;
            }
            if (!valType_.IsConvertibleTo(requiredType_)) {
                throw new SyntaxErrorException(
                    $"{clauseName} clause's value must be convertible to {requiredType_}", clause
                );
            }
        }
        if (!(isEnumeratingLoop || isInLoop || clauseNames.Contains("to"))) {
            throw new SyntaxErrorException(
                $"For conditions must have a to, in, or enumerating clause", this
            );
        }
    }
    
    public int Serialize(SerializationContext context) {
        Type_ iterType_ = type_;
        SerializationContext loop = context.AddSubContext();
        loop.Serialize(block);
        loop.AddDeclaration(declarationID);
        List<int> clauseIdxs = new List<int>();
        JSONList clauseNames = new JSONList();
        foreach (ForClause clause in clauses) {
            string clauseName = clause.GetName();
            if (clauseName == "in") {
                iterType_ = new Type_("W", 64);
            }
            clauseIdxs.Add(context.SerializeInstruction(clause.GetValue()));
            clauseNames.Add(new JSONString(clauseName));
        }
        Function func = TokenUtils.GetParentOfType<Function>(this);
        int iterVar = func.AddSpecialAlloc(iterType_);
        return context.AddInstruction(
            new SerializableInstruction("for", clauseIdxs)
                .AddData("block", new JSONInt(loop.GetIndex()))
                .AddData("variable", new JSONInt(declarationID))
                .AddData("clause_names", clauseNames)
                .AddData("type_", type_.GetJSON())
                .AddData("iter_type_", iterType_.GetJSON())
                .AddData("iter_var", new JSONInt(iterVar))
        );
    }

    public CodeBlock GetBlock() {
        return block;
    }
}
