public class Conditional : IFlowControl, IFunctionTerminator {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly List<Condition> conditions;
    CodeBlock elseBlock;

    public int Count {
        get => conditions.Count + (elseBlock==null?0:1);
    }

    public IToken this[int i] {
        get => i == conditions.Count ? elseBlock : conditions[i];
        set {
            if (i == conditions.Count) {
                elseBlock = (CodeBlock)value;
            } else {
                conditions[i] = (Condition)value;
            }
        }
    }

    public Conditional(IValueToken condition, CodeBlock block) {
        Condition cond = new(condition, block);
        cond.span = TokenUtils.MergeSpans(condition, block);
        conditions = [cond];
    }

    public Conditional(Conditional conditional, IValueToken condition, CodeBlock block) {
        conditions = new List<Condition>(conditional.GetConditions());
        Condition cond = new(condition, block);
        if (conditional.GetElseBlock() != null) {
            throw new SyntaxErrorException(
                "Cannot add condition to conditional already terminated with else block", cond
            );
        }
        cond.span = TokenUtils.MergeSpans(condition, block);
        conditions.Add(cond);
    }

    public Conditional(Conditional conditional, CodeBlock elseBlock) {
        if (conditional.GetElseBlock() != null) {
            throw new SyntaxErrorException(
                "Cannot add else to conditional already terminated with else block", elseBlock
            );
        }
        conditions = conditional.GetConditions();
        this.elseBlock = elseBlock;
    }

    public Conditional(List<Condition> conditions, CodeBlock elseBlock=null) {
        this.conditions = conditions;
        this.elseBlock = elseBlock;
    }

    public List<Condition> GetConditions() {
        return conditions;
    }

    public CodeBlock GetElseBlock() {
        return elseBlock;
    }

    public override string ToString() {
        string result = string.Join(", ", conditions);
        if (elseBlock != null) {
            result += ", Else: " + elseBlock.ToString();
        }
        return Utils.WrapName(GetType().Name, result);
    }

    public int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["conditions"] = conditions.Select(condition => new Dictionary<string, object> {
                {"block", condition.GetBlock()}, {"condition", condition.GetCondition()}
            }),
            ["else"] = elseBlock
        }.Register();
    }

    public bool DoesTerminateFunction() {
        if (elseBlock == null) return false;
        if (!elseBlock.DoesTerminateFunction()) return false;
        return conditions.All(condition =>
            condition.GetBlock().DoesTerminateFunction());
    }
}
