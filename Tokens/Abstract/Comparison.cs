namespace Epsilon;
public abstract class Comparison(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken, IVerifier {
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

    public override int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["common_type_"] = Type_.CommonNonNull(this, o1.GetType_(), o2.GetType_())
        }.SetOperands([o1, o2]).Register();
    }
}
