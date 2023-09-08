using System;
using System.Collections.Generic;

public class Not : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    
    IValueToken o;
    
    public int Count {
        get { return 1; }
    }
    
    public IToken this[int i] {
        get {
            return o;
        }
        set {
            o = (IValueToken)value;
        }
    }
    
    public Not(IValueToken o) {
        this.o = o;
    }

    public override string ToString() {
        return Utils.WrapName(
            this.GetType().Name, o.ToString()
        );
    }

    public Type_ GetType_() {
        return new Type_("Bool");
    }
}
