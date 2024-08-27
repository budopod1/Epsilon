public interface IParentToken : IMultiLineToken {
    IToken this[int i] {
        get;
        set;
    }

    int Count {
        get;
    }
}
