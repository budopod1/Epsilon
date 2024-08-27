public class PostDecrement(IAssignableValue o) : UnaryOperation<IAssignableValue>(o), IValueToken {
    public Type_ GetType_() {
        return o.GetType_();
    }

    public override int Serialize(SerializationContext context) {
        IValueToken newValue = new SubOne(o);
        ICompleteLine line = o.AssignTo(newValue);
        line.parent = this;
        newValue.parent = (IParentToken)line;
        new SerializableInstruction(context, line).Register();
        return new SerializableInstruction(context, o).Register();
    }
}
