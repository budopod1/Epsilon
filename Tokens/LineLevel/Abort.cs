namespace Epsilon;
public class Abort(ValueList list) : UnaryOperation<IValueToken>(GetValueFromList(list)), IFunctionTerminator, IBlockEndOnly, ICanAbort {
    static IValueToken GetValueFromList(ValueList list) {
        if (list.Count != 1) {
            throw new SyntaxErrorException(
                "Expected exactly one value", list[1]
            );
        }
        ValueListItem listItem = (ValueListItem)list[0];
        if (listItem.Count != 1) {
            throw new SyntaxErrorException(
                "Invalid syntax in abort parameter", listItem
            );
        }
        if (listItem[0] is not IValueToken itemValue) {
            throw new SyntaxErrorException(
                "Invalid syntax in abort parameter", listItem
            );
        }
        return itemValue;
    }

    public bool CanAbort() {
        return true;
    }

    public bool DoesTerminateFunction() {
        return true;
    }
}
