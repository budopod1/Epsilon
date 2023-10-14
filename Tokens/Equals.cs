using System;

public class Equals : BinaryOperation<IValueToken, IValueToken>, IValueToken, IVerifier {
    public Equals(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return new Type_("Bool");
    }

    public void Verify() {
        if (!Type_.AreCompatible(o1.GetType_(), o2.GetType_())) {
            throw new SyntaxErrorException(
                $"Cannot check equality of {o1.GetType_()} and {o2.GetType_()}"
            );
        }
    }
}
