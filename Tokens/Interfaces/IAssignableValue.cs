namespace Epsilon;
public interface IAssignableValue : IValueToken {
    IAssignment AssignTo(IValueToken value);
}
