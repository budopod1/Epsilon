using System;
using System.Collections.Generic;

public class Assignment : ParentToken {
    Name variable;
    IValueToken value;
    
    public override int Count {
        get { return 2; }
    }
    
    public override Token this[int i] {
        get {
            if (i == 0) return variable;
            return value;
        }
        set {
            if (i == 0) {
                variable = (Name)value;
            } else {
                this.value = (IValueToken)value;
            }
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
