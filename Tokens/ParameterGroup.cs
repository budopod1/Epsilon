using System;
using System.Collections.Generic;

public class ParameterGroup : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    
    public int Count {
        get { return 1; }
    }
    
    public IToken this[int i] {
        get { return child; }
        set { child = (IValueToken)value; }
    }

    IValueToken child;
    
    public ParameterGroup(IValueToken child) {
        this.child = child;
    }
    
    public Type_ GetType_() {
        return child.GetType_();
    }

    public override string ToString() {
        return Utils.WrapName(this.GetType().Name, child.ToString());
    }
}