using System;
using System.Collections.Generic;

public class Assignment : IParentToken {
    Name variable;
    IValueToken value;
    
    public int Count {
        get { return 2; }
    }
    
    public IToken this[int i] {
        get {
            if (i == 0) return variable;
            return value;
        }
        set {
            if (i == 0) variable = (Name)value;
            this.value = (IValueToken)value;
        }
    }
    
    public Assignment(Name variable, IValueToken value) {
        this.variable = variable;
        this.value = value;
    }

    public override string ToString() {
        return Utils.WrapName(
            this.GetType().Name, $"{variable.ToString()}, {value.ToString()}"
        );
    }
}
