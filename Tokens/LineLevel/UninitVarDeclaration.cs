using System;
using System.Collections.Generic;

public class UninitVarDeclaration : ICompleteLine, ISerializableToken, IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    int id;
    
    public UninitVarDeclaration(VarDeclaration declaration) {
        id = declaration.GetID();
    }

    public void Verify() {
        Scope scope = Scope.GetEnclosing(this);
        ScopeVar svar = scope.GetVarByID(id);
        Type_ varType_ = svar.GetType_();
        if (!varType_.GetBaseType_().IsValueType_()) {
            throw new SyntaxErrorException(
                $"Non-value type {svar} cannot be declared without an initial value", this
            );
        }
    }

    public int Serialize(SerializationContext context) {
        context.AddDeclaration(id);
        return context.AddInstruction(
            new SerializableInstruction(this).AddData("variable", new JSONInt(id))
        );
    }
}
