using System;
using System.Collections.Generic;

public class MemberAccess : UnaryOperation<IValueToken>, IAssignableValue, IVerifier {
    string member;
    Type_ structType_;

    public MemberAccess(IValueToken o, MemberAccessPostfix member) : base(o) {
        this.member = member.GetValue();
        structType_ = o.GetType_().UnwrapPoly();
    }

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

    public override int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
                .AddData("member", new JSONString(member))
                .AddData("struct_type_", structType_.GetJSON())
        );
    }

    public ICompleteLine AssignTo(IValueToken value) {
        return new MemberAssignment(this, value);
    }
}
