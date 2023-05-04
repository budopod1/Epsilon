using System;

public class Unit : IToken {
    public Object value;
    
    public Unit(Object value) {
        this.value = value;
    }

    public override string ToString() {
        return Utils.WrapName(this.GetType().Name, this.value.ToString());
    }
}
