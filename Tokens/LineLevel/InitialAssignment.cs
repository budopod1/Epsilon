using System;
using System.Collections.Generic;

public class InitialAssignment : UnaryOperation<IValueToken>, IVerifier, ICompleteLine, ISerializableToken {
    int id;
    
    public InitialAssignment(VarDeclaration declaration, IValueToken o) : base(o) {
        id = declaration.GetID();
    }

    public void Verify() {
        Type_ valueType_ = o.GetType_();
        Scope scope = Scope.GetEnclosing(this);
        ScopeVar svar = scope.GetVarByID(id);
        Type_ varType_ = svar.GetType_();
        if (!valueType_.IsConvertibleTo(varType_)) {
            throw new SyntaxErrorException(
                $"Cannot assign value of type {valueType_} to variable of type {varType_}", this
            );
        }
    }

    public override int Serialize(SerializationContext context) {
        context.AddDeclaration(id);
        Scope scope = Scope.GetEnclosing(this);
        ScopeVar svar = scope.GetVarByID(id);
        return context.AddInstruction(
            new SerializableInstruction(this, context)
                .AddData("variable", new JSONInt(id))
                .AddData("var_type_", svar.GetType_().GetJSON())
        );
    }
}
