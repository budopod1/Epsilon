using System;
using System.Linq;
using System.Collections.Generic;

public class Conditional : IFlowControl, IFunctionTerminator {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    List<Condition> conditions;
    CodeBlock elseBlock;

    public int Count {
        get { return conditions.Count + (elseBlock==null?0:1); }
    }

    public IToken this[int i] {
        get {
            if (i == conditions.Count) return elseBlock;
            return conditions[i];
        }
        set {
            if (i == conditions.Count) {
                elseBlock = (CodeBlock)value;
            } else {
                conditions[i] = (Condition)value;
            }
        }
    }

    public Conditional(IValueToken condition, CodeBlock block) {
        Condition cond = new Condition(condition, block);
        cond.span = TokenUtils.MergeSpans(condition, block);
        conditions = new List<Condition> {cond};
    }

    public Conditional(Conditional conditional, IValueToken condition, CodeBlock block) {
        conditions = new List<Condition>(conditional.GetConditions());
        Condition cond = new Condition(condition, block);
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
        string result = String.Join(", ", conditions);
        if (elseBlock != null) {
            result += ", Else: " + elseBlock.ToString();
        }
        return Utils.WrapName(GetType().Name, result);
    }

    public int Serialize(SerializationContext context) {
        JSONList conditionsJSON = new JSONList();
        foreach (Condition condition in conditions) {
            SerializationContext sub = context.AddSubContext();
            sub.Serialize(condition.GetBlock());
            JSONObject conditionObj = new JSONObject();
            conditionObj["block"] = new JSONInt(sub.GetIndex());
            SerializationContext conditionCtx = context.AddSubContext(hidden: true);
            conditionCtx.SerializeInstruction(condition.GetCondition());
            conditionObj["condition"] = conditionCtx.Serialize();
            conditionsJSON.Add(conditionObj);
        }
        IJSONValue elseJSON = new JSONNull();
        if (elseBlock != null) {
            SerializationContext sub = context.AddSubContext();
            sub.Serialize(elseBlock);
            elseJSON = new JSONInt(sub.GetIndex());
        }
        return context.AddInstruction(
            new SerializableInstruction(this)
                .AddData("conditions", conditionsJSON)
                .AddData("else", elseJSON)
        );
    }

    public bool DoesTerminateFunction() {
        if (elseBlock == null) return false;
        if (!elseBlock.DoesTerminateFunction()) return false;
        return conditions.All(condition =>
            condition.GetBlock().DoesTerminateFunction());
    }
}
