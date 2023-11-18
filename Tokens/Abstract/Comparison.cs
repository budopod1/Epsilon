using System;

public abstract class Comparison : BinaryOperation<IValueToken, IValueToken>, IValueToken, IVerifier {
    public Comparison(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        return new Type_("Bool");
    }

    public void Verify() {
        if (!Type_.AreCompatible(o1.GetType_(), o2.GetType_())) {
            throw new SyntaxErrorException(
                $"Cannot check equality of {o1.GetType_()} and {o2.GetType_()}", this
            );
        }
    }

    public override int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
                .AddData("common_type_", Type_.Common(o1.GetType_(), o2.GetType_()).GetJSON())
        );
    }
}
