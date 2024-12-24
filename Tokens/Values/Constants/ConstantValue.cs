namespace Epsilon;
public class ConstantValue(IConstant constant) : Unit<IConstant>(constant), IValueToken {
    public Type_ GetType_() {
        return GetValue().GetType_();
    }

    public int UncachedSerialize(SerializationContext context) {
        if (GetValue() is StringConstant) {
            throw new InvalidOperationException(
                "String constants cannot appear in the final IR"
            );
        }
        return new SerializableInstruction(context, this) {
            ["constant"] = GetValue()
        }.Register();
    }
}
