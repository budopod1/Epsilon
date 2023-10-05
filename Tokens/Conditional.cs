using System;
using System.Collections.Generic;

public class Conditional : IParentToken {
    public IParentToken parent { get; set; }
    
    List<Condition> conditions;
    CodeBlock elseBlock;

    public int Count {
        get { return conditions.Count + (elseBlock==null?0:1); }
    }
    
    public IToken this[int i] {
        get {
            if (i == conditions.Count) {
                return elseBlock;
            }
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
        conditions = new List<Condition> {new Condition(
            condition, block
        )};
    }

    public Conditional(Conditional conditional, IValueToken condition, CodeBlock block) {
        conditions = new List<Condition>(conditional.GetConditions());
        conditions.Add(new Condition(condition, block));
    }

    public Conditional(Conditional conditional, CodeBlock elseBlock) {
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
        return Utils.WrapName(this.GetType().Name, result);
    }
}