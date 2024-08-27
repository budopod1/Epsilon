public class Assignment(Variable variable, IValueToken o) : UnaryOperation<IValueToken>(o), IVerifier, ICompleteLine, ISerializableToken {
    readonly int id = variable.GetID();

    public void Verify() {
        Type_ valueType_ = o.GetType_();
        ScopeVar svar = Scope.GetVarByID(this, id);
        Type_ varType_ = svar.GetType_();
        if (!valueType_.IsConvertibleTo(varType_)) {
            throw new SyntaxErrorException(
                $"Cannot assign value of type {valueType_} to variable of type {varType_}", this
            );
        }
    }

    public override int Serialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["variable"] = id,
            ["var_type_"] = Scope.GetVarByID(this, id).GetType_()
        }.SetOperands([o]).Register();
    }
}
