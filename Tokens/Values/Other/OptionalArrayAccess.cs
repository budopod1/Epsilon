public class OptionalArrayAccess : BinaryOperation<IValueToken, IValueToken>, IValueToken, IVerifier {
    public OptionalArrayAccess(IValueToken array, IValueToken index) : base(array, index) {}
    public OptionalArrayAccess(IValueToken array, ValueList index) : base(array, null) {
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
        Type_ elemType_ = o1.GetType_().GetGeneric(0);
        if (elemType_.GetBaseType_().IsOptionable()) {
            return elemType_.OptionalOf();
        } else {
            throw new SyntaxErrorException(
                $"Cannot peform optional array access on array containing non-optionable type {elemType_}", this
            );
        }
    }

    public void Verify() {
        GetType_();
        if (!o2.GetType_().IsConvertibleTo(new Type_("Z"))) {
            throw new SyntaxErrorException(
                $"Arrays can only be indexed with integers, not {o2.GetType_()}", this
            );
        }
    }
}
