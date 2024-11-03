namespace Epsilon;
public class MemberAccess(IValueToken o, MemberAccessPostfix member) : UnaryOperation<IValueToken>(o), IAssignableValue, IVerifier {
    readonly string member = member.GetValue();
    readonly Type_ structType_ = o.GetType_().UnwrapPoly();

    public Type_ GetType_() {
        Type_ type_ = o.GetType_();
        Struct struct_ = StructsCtx.GetStructFromType_(structType_);
        if (struct_ == null)
            throw new SyntaxErrorException(
                $"You can access members of struct and poly types, not {type_}", this
            );
        Field field = struct_.GetField(member);
        if (field == null)
            throw new SyntaxErrorException(
                $"Struct {struct_.GetName()} has no member {member}", this
            );
        return field.GetType_();
    }

    public string GetMember() {
        return member;
    }

    public override string ToString() {
        return Utils.WrapName(
            GetType().Name,  o.ToString() + ", " + member
        );
    }

    public void Verify() {
        GetType_();
    }

    public IAssignment AssignTo(IValueToken value) {
        return new MemberAssignment(this, value);
    }

    public override int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["member"] = member,
            ["struct_type_"] = structType_
        }.SetOperands([o]).Register();
    }
}
