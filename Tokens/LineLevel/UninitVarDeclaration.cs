public class UninitVarDeclaration(VarDeclaration declaration) : ICompleteLine, ISerializableToken, IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly int id = declaration.GetID();

    public void Verify() {
        ScopeVar svar = Scope.GetVarByID(this, id);
        Type_ varType_ = svar.GetType_();
        if (!varType_.GetBaseType_().IsValueType_()) {
            throw new SyntaxErrorException(
                $"Non-value type {varType_} cannot be declared without an initial value", this
            );
        }
    }

    public int UncachedSerialize(SerializationContext context) {
        context.RegisterVarDecl(id);
        return new SerializableInstruction(context, this) {
            ["variable"] = id
        }.Register();
    }
}
