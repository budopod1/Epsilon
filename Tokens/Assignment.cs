using System;
using System.Collections.Generic;

public class Assignment : IParentToken {
    public IParentToken parent { get; set; }
    
    Variable variable;
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
            if (i == 0) {
                variable = (Variable)value;
            } else {
                this.value = (IValueToken)value;
            }
        }
    }
    
    public Assignment(Variable variable, IValueToken value) {
        this.variable = variable;
        this.value = value;
    }

    public override string ToString() {
        return Utils.WrapName(
            this.GetType().Name, $"{variable.ToString()}, {value.ToString()}"
        );
    }
}
