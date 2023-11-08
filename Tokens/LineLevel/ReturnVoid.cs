using System;

public class ReturnVoid : IVerifier, ICompleteLine, IBlockEndOnly {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    public void Verify() {
        Function func = TokenUtils.GetParentOfType<Function>(this);
        Type_ returnType_ = func.GetReturnType_();
        if (!func.GetReturnType_().GetBaseType_().IsVoid()) {
            throw new SyntaxErrorException(
                $"Cannot return void; function expects {returnType_} return type", this
            );
        }
    }

    public override string ToString() {
        return GetType().Name + "()";
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(new SerializableInstruction(this));
    }
}
