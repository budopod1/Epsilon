using System;
using System.Collections.Generic;

public class Type_ {
// https://en.wikipedia.org/wiki/Set_(mathematics)#Special_sets_of_numbers_in_mathematics
    public static List<string> BuiltInTypes_ = new List<string> {
        "Unkown",
        "Void",
        "Bool",
        "Byte",
        "W", // whole numbers
        "Z",
        "Q",
        "Array",
        "Struct",
    };

    public static List<KeyValuePair<string, string>> BuiltInTypes_Decent = 
        new List<KeyValuePair<string, string>> {
            new KeyValuePair<string, string>("Unkown", null),
            new KeyValuePair<string, string>("Void", null),
            new KeyValuePair<string, string>("Unkown", null),
            new KeyValuePair<string, string>("Bool", null),
            new KeyValuePair<string, string>("Byte", "W"),
            new KeyValuePair<string, string>("W", "Z"),
            new KeyValuePair<string, string>("Z", "Q"),
            new KeyValuePair<string, string>("Array", null),
            new KeyValuePair<string, string>("Struct", null),
        };
    
    string name;
    List<Type_> generics;

    public Type_(string name, List<Type_> generics) {
        this.name = name;
        this.generics = generics;
    }

    public Type_(string name) {
        this.name = name;
        this.generics = new List<Type_>();
    }

    public Type_ WithGenerics(List<Type_> generics) {
        return new Type_(this.name, generics);
    }

    public string GetName() {
        return name;
    }

    public List<Type_> GetGenerics() {
        return generics;
    }

    public override string ToString() {
        string genericStr = "";
        bool first = true;
        foreach (Type_ generic in generics){
            if (!first) {
                genericStr += ", ";
            }
            genericStr += generic.ToString();
            first = false;
        }
        return Utils.WrapName(name, genericStr, "<", ">");
    }
}

/*
using System;

public enum Primitive {
    Unkown,
    Void,
    Bool,
    Byte,
    W, // unsigned integers
    Z, // signed integers
    Q, // floats
    Bytes,
}
*/
/*
using System;

// To explain: the type of the type_
public enum Type_Type {
    Primitive,
    Struct
}
*/
