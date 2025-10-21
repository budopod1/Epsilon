using CsJSONTools;

namespace Epsilon;
public class For(RawFor source) : IParentToken, ILoop, IVerifier, ISerializableToken {
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
                block = (CodeBlock)value;
            } else {
                clauses[i-1] = (ForClause)value;
            }
        }
    }

    readonly List<ForClause> clauses = source.GetClauses().Select(
            clause => new ForClause(clause)
        ).ToList();
    readonly int declarationID = source.GetDeclarationID();
    readonly Type_ type_ = source.GetType_();
    CodeBlock block = source.GetBlock();

    public CodeBlock GetBlock() {
        return block;
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
            if (!type_.GetBaseType_().IsInt()) {
                throw new SyntaxErrorException(
                    "Non-in for loop variable type must be an integer", this
                );
            }
        }
        foreach (ForClause clause in clauses) {
            Type_ valType_ = clause.GetValue().GetType_();
            string clauseName = clause.GetName();
            if (clauseName == "to" && isEnumeratingLoop) {
                throw new SyntaxErrorException(
                    "Enumerating for loops cannot have a to clause", clause
                );
            }
            if (clauseName == "to" || clauseName == "from") {
                if (isInLoop && !valType_.GetBaseType_().IsUnsignedInt()) {
                    throw new SyntaxErrorException(
                        $"The {clauseName} clause expects an unsigned int, found value of type {valType_}", clause
                    );
                } else if (!isInLoop && !valType_.IsConvertibleTo(type_)) {
                    throw new SyntaxErrorException(
                        $"The {clauseName} clause expects a value convertible to {type_}, found value of type {valType_}", clause
                    );
                }
            }
            if (clauseName == "step" && !valType_.GetBaseType_().IsInt()) {
                throw new SyntaxErrorException(
                    $"The step clause expects an integer, found value of type {valType_}", clause
                );
            }
            if (clauseName == "in" && !valType_.IsConvertibleTo(type_.ArrayOf())) {
                throw new SyntaxErrorException(
                    $"The in clause expects a value convertible to {type_.ArrayOf()}, found value of type {valType_}", clause
                );
            }
            if (clauseName == "enumerating" && valType_.GetBaseType_().GetName() != "Array") {
                throw new SyntaxErrorException(
                    $"The enumerating clause expects an array, found value of type {valType_}", clause
                );
            }
        }
        if (!(isEnumeratingLoop || isInLoop || clauseNames.Contains("to"))) {
            throw new SyntaxErrorException(
                $"For conditions must have a to, in, or enumerating clause", this
            );
        }
    }

    public int UncachedSerialize(SerializationContext context) {
        Type_ iterType_ = type_;
        if (clauses.Any(clause => clause.GetName() == "in"))
            iterType_ = new Type_("W", 64);
        Function func = TokenUtils.GetParentOfType<Function>(this);
        int iterVar = func.AddSpecialAlloc(iterType_);
        IJSONValue serializedBlock = SerializationContext.SerializeBlock(
            context, block, [declarationID]);
        return new SerializableInstruction(context, this) {
            ["block"] = serializedBlock,
            ["variable"] = declarationID,
            ["clause_names"] = clauses.Select(clause => clause.GetName()),
            ["var_type_"] = type_,
            ["idx_type_"] = iterType_,
            ["iter_alloc"] = iterVar
        }.SetOperands(clauses.Select(clause => clause.GetValue())).Register();
    }

    public override string ToString() {
        List<string> sub = [];
        for (int i = 0; i < Count; i++) {
            sub.Add(this[i].ToString());
        }
        return Utils.WrapName("For", string.Join(", ", sub));
    }
}
