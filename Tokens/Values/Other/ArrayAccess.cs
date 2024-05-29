using System;

public class ArrayAccess : BinaryOperation<IValueToken, IValueToken>, IAssignableValue, IVerifier {
    public ArrayAccess(IValueToken array, IValueToken index) : base(array, index) {}
    public ArrayAccess(IValueToken array, ValueList index) : base(array, null) {
        if (index.Count != 1)
            throw new SyntaxErrorException(
                $"Arrays can only be indexed with 1 value, not {index.Count} values", index
            );
        ValueListItem indexListItem = (ValueListItem)index[0];
        if (indexListItem.Count != 1 || !(indexListItem[0] is IValueToken))
            throw new SyntaxErrorException(
                $"Malformed array index", indexListItem
            );
        o2 = (IValueToken)indexListItem[0];
    }

    public IValueToken GetArray() {
        return o1;
    }

    public IValueToken GetIndex() {
        return o2;
    }

    public Type_ GetType_() {
        return o1.GetType_().GetGeneric(0);
    }

    public void Verify() {
        if (!o2.GetType_().IsConvertibleTo(new Type_("Z"))) {
            throw new SyntaxErrorException(
                $"Arrays can only be indexed with integers, not {o2.GetType_()}", this
            );
        }
    }

    public ICompleteLine AssignTo(IValueToken value) {
        return new ArrayAssignment(this, value);
    }
}
