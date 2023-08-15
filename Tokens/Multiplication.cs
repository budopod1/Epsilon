using System;
using System.Collections.Generic;

public class Multiplication : IParentToken, IValueToken {
    IValueToken o1;
    IValueToken o2;
    
    public int Count {
        get { return 2; }
    }
    
    public IToken this[int i] {
        get {
            if (i == 0) return o1;
            return o2;
        }
        set {
            if (i == 0) {
                o1 = (IValueToken)value;
            } else {
                o2 = (IValueToken)value;
            }
        }
    }
    
    public Multiplication(IValueToken o1, IValueToken o2) {
        this.o1 = o1;
        this.o2 = o2;
    }

    public override string ToString() {
        return Utils.WrapName(
            this.GetType().Name, $"{o1.ToString()}, {o2.ToString()}"
        );
    }

    public Type_ GetType_() {
        return Type_.Unknown(); // temp
    }
}
