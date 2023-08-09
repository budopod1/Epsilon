using System;

public interface IParentToken : IToken {
    IToken this[int i] {
        get;
        set;
    }

    int Count {
        get;
    }
}
