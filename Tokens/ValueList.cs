using System;
using System.Collections.Generic;

public class ValueList : IParentToken {
    public IParentToken parent { get; set; }
    
    public int Count {
        get { return values.Count; }
    }
    
    public IToken this[int i] {
        get { return values[i]; }
        set { values[i] = value; }
    }
    
    List<IToken> values;
    
    public ValueList(List<IToken> values) {
        this.values = values;
    }

    public override string ToString() {
        string result = "";
        foreach (IToken value in values) {
            result += value.ToString() + ",";
        }
        return Utils.WrapName(
            this.GetType().Name, result
        );
    }
}
