using System;
using System.Collections.Generic;

public class Struct : IMultiLineToken, ITopLevel {
    public IParentToken parent { get; set; }
    
    string name;
    List<Field> fields;
    
    public Struct(string name, List<Field> fields) {
        this.name = name;
        this.fields = fields;
    }

    public string GetName() {
        return name;
    }

    public List<Field> GetFields() {
        return fields;
    }

    public Field GetField(string name) {
        foreach (Field field in fields) {
            if (field.GetName() == name)
                return field;
        }
        return null;
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
