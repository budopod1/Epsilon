using System;
using System.Linq;
using System.Collections.Generic;

public class Struct : IEquatable<Struct> {
    LocatedID id;
    List<Field> fields;
    string symbol;

    public Struct(string path, string name, List<Field> fields, List<IAnnotation> annotations) {
        id = new LocatedID(path, name);
        this.fields = fields;
        symbol = id.GetID();
        foreach (IAnnotation annotation in annotations) {
            IDAnnotation idA = annotation as IDAnnotation;
            if (idA != null) {
                symbol = idA.GetID();
            }
        }
    }

    public Struct(string path, string name, List<Field> fields, string symbol) {
        id = new LocatedID(path, name);
        this.fields = fields;
        this.symbol = symbol;
    }

    public string GetPath() {
        return id.Path;
    }

    public string GetName() {
        return id.Name;
    }

    public string GetID() {
        return id.GetID();
    }

    public string GetSymbol() {
        return symbol;
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
        string result = $"Name: {GetName()}, Path: {GetPath()}";
        foreach (Field field in fields) {
            result += "\n" + field.ToString();
        }
        return Utils.WrapName(
            "Struct", Utils.WrapNewline(Utils.Indent(result))
        );
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["id"] = new JSONString(GetID());
        obj["name"] = new JSONString(GetName());
        obj["fields"] = new JSONList(fields.Select(
            field => field.GetJSON()
        ));
        obj["symbol"] = new JSONString(symbol);
        return obj;
    }

    public bool Equals(Struct other) {
        if (GetID() != other.GetID()) return false;
        if (GetSymbol() != other.GetSymbol()) return false;
        return Utils.ListEqual(fields, other.GetFields());
    }
}
