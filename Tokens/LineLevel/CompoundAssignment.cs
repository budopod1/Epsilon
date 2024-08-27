public class CompoundAssignment(Type type, IAssignableValue o1, IValueToken o2) : BinaryOperation<IAssignableValue, IValueToken>(o1, o2), ICompleteLine {
    readonly Type type = type;

    public override int Serialize(SerializationContext context) {
        IValueToken newValue = (IValueToken)Activator.CreateInstance(
            type, [o1, o2]
        );
        ICompleteLine line = o1.AssignTo(newValue);
        line.parent = this;
        newValue.parent = (IParentToken)line;
        return new SerializableInstruction(context, line).Register();
    }
}
