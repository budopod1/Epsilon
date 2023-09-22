using System;
using System.Linq;
using System.Collections.Generic;

public class ValueList : IParentToken {
    public IParentToken parent { get; set; }
    
    public int Count {
        get { return values.Count; }
    }
    
    public IToken this[int i] {
        get { return values[i]; }
        set { values[i] = ((ValueListItem)value); }
    }
    
    List<ValueListItem> values;
    
    public ValueList(List<IToken> values) {
        this.values = values.Cast<ValueListItem>().ToList();
    }
    
    public ValueList(List<ValueListItem> values) {
        this.values = values;
    }

    public List<ValueListItem> GetValues() {
        return values;
    }

    public override string ToString() {
        string result = "";
        foreach (ValueListItem value in values) {
            result += value.ToString() + ",";
        }
        return Utils.WrapName(
            this.GetType().Name, result
        );
    }
}
