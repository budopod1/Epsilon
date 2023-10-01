using System;

public class IfElse : TrinaryOperation<IValueToken, CodeBlock, CodeBlock> {
    public IfElse(IValueToken condition, CodeBlock block1, CodeBlock block2) : base(condition, block1, block2) {}
    
    public IfElse(If if_, CodeBlock elseBlock) : base(if_.GetCondition(), if_.GetIfBlock(), elseBlock) {}

    public IValueToken GetCondition() {
        return o1;
    }

    public CodeBlock GetIfBlock() {
        return o2;
    }

    public CodeBlock GetElseBlock() {
        return o3;
    }
}
