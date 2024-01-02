using System;
using System.Reflection;

public class CompoundAssignment : BinaryOperation<IAssignableValue, IValueToken>, ICompleteLine {
    Type type;
    
    public CompoundAssignment(Type type, IAssignableValue o1, IValueToken o2) : base(o1, o2) {
        this.type = type;
    }

    public override int Serialize(SerializationContext context) {
        IValueToken newValue = (IValueToken)Activator.CreateInstance(
            type, new object[] {o1, o2}
        );
        ICompleteLine line = o1.AssignTo(newValue);
        line.parent = this;
        newValue.parent = (IParentToken)line;
        return context.SerializeInstruction(line);
    }
}
