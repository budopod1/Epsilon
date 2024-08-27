public interface IAssignableValue : IValueToken {
    ICompleteLine AssignTo(IValueToken value);
}
