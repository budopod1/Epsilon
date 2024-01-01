using System;
using System.Collections.Generic;

public class Assignment : BinaryOperation<Variable, IValueToken>, IVerifier, ICompleteLine, ISerializableToken {
    public Assignment(Variable o1, IValueToken o2) : base(o1, o2) {}

    public void Verify() {
        if (!o2.GetType_().IsConvertibleTo(o1.GetType_())) {
            throw new SyntaxErrorException(
                $"Cannot assign value of type {o2.GetType_()} to variable of type {o1.GetType_()}", this
            );
        }
    }

    public override int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(
                "assignment", new List<int> {o2.Serialize(context)}
            ).AddData("variable", new JSONInt(o1.GetID()))
             .AddData("var_type_", o2.GetType_().GetJSON())
        );
    }
}
