using System;

public class Assignment : BinaryOperation<Variable, IValueToken>, IVerifier, ICompleteLine {
    public Assignment(Variable o1, IValueToken o2) : base(o1, o2) {}

    public void Verify() {
        if (!o2.GetType_().IsConvertibleTo(o1.GetType_())) {
            throw new SyntaxErrorException(
                $"Cannot assign value of type {o2.GetType_()} to variable of type {o1.GetType_()}"
            );
        }
    }
}
