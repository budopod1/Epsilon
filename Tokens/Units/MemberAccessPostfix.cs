using System;

public class MemberAccessPostfix : Unit<string> {
    public MemberAccessPostfix(string member) : base(member) {}
    public MemberAccessPostfix(Name name) : base(name.GetValue()) {}
}
