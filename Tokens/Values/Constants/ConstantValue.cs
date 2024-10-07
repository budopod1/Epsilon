public class ConstantValue(IConstant constant) : Unit<IConstant>(constant), IValueToken {
    public Type_ GetType_() {
        return GetValue().GetType_();
    }

    public int UncachedSerialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["constant"] = GetValue()
        }.Register();
    }
}
