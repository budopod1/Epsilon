using System;

public abstract class ParentToken : Token, IMultiLineToken {
    public abstract Token this[int i] {
        get;
        set;
    }

    public abstract int Count {
        get;
    }
}
