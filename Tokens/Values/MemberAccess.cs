using System;

public class MemberAccess : UnaryOperation<IValueToken>, IValueToken, IVerifier {
    string member;
    
    public MemberAccess(IValueToken o, Name member) : base(o) {
        this.member = member.GetValue();
    }

    public Type_ GetType_() {
        Program program = TokenUtils.GetParentOfType<Program>(this);
        Type_ type_ = o.GetType_();
        Struct struct_ = program.GetStructFromType_(type_);
        if (struct_ == null)
            throw new SyntaxErrorException(
                $"You can access members of struct types, not {type_}"
            );
        Field field = struct_.GetField(member);
        if (field == null)
            throw new SyntaxErrorException(
                $"Struct {struct_.GetName()} has no member {member}"
            );
        return field.GetType_();
    }

    public override string ToString() {
        return Utils.WrapName(
            this.GetType().Name, 
            o.ToString() + ", " + member
        );
    }

    public void Verify() {
        GetType_();
    }
}
