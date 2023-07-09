using System;
using System.Collections.Generic;

public class Type_ {
    public static List<string> BuiltInTypes_ = new List<string> {
        "Unkown",
        "Void",
        "Bool",
        "Byte",
        "W",
        "Z",
        "Q",
        "Array",
        "Struct",
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
    // https://en.wikipedia.org/wiki/Set_(mathematics)#Special
    // _sets_of_numbers_in_mathematics
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
