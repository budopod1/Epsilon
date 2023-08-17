using System;
using System.Collections.Generic;

public class Struct : IMultiLineToken {
    public IParentToken parent { get; set; }
    
    string name;
    List<Field> fields;
    
    public Struct(string name, List<Field> fields) {
        this.name = name;
        this.fields = fields;
    }

    public override string ToString() {
        string result = $"Name: {name}";
        foreach (Field field in fields) {
            result += "\n" + field.ToString();
        }
        return Utils.WrapName(
            "Struct", 
            Utils.WrapNewline(Utils.Indent(result))
        );
    }
}
