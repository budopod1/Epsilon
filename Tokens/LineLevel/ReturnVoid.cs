using System;

public class ReturnVoid : IVerifier, IFunctionTerminator, IBlockEndOnly {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    public void Verify() {
        Function func = TokenUtils.GetParentOfType<Function>(this);
        if (!func.DoesReturnVoid()) {
            throw new SyntaxErrorException(
                $"Cannot return nothing; function expects {func.GetReturnType_()} return type", this
            );
        }
    }

    public override string ToString() {
        return GetType().Name + "()";
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(new SerializableInstruction(this));
    }

    public bool DoesTerminateFunction() {
        return true;
    }
}
