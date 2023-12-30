using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class UserBaseType_ {
    string name;
    int? bits;

    public UserBaseType_(string name, int? bits = null) {
        this.name = name;
        this.bits = bits;
    }

    public static List<string> SpecialFullBaseType_Names = new List<string> {
        "Str", "L"
    };

    public static List<string> NonUserType_Names = new List<string> {
        "Null"
    };

    public static UserBaseType_ ParseString(string content, List<string> structNames) {
        if ((structNames.Contains(content) || BaseType_.BuiltInTypes_.Contains(content)
            || SpecialFullBaseType_Names.Contains(content)) 
            && !NonUserType_Names.Contains(content)) {
            return new UserBaseType_(content);
        }
        System.Text.RegularExpressions.Match match = Regex.Match(
            content, $@"({String.Join('|', BaseType_.BitTypes_)})(\d+)"
        );
        if (match.Success) {
            string name = match.Groups[1].Value;
            if (NonUserType_Names.Contains(name)) return null;
            int bits = Int32.Parse(match.Groups[2].Value);
            return new UserBaseType_(name, bits);
        }
        return null;
    }

    static Dictionary<string, Type_> SpecialFullBaseTypes_ = new Dictionary<string, Type_> {
        {"Str", Type_.String()}, {"L", new Type_("W", 64)}
    };

    public Type_ ToType_(List<Type_> generics) {
        if (SpecialFullBaseType_Names.Contains(name)) {
            if (generics.Count > 0) {
                throw new IllegalType_GenericsException(
                    $"Special type {name} cannot have generics"
                );
            }
            return SpecialFullBaseTypes_[name];
        }
        return new Type_(new BaseType_(name, bits), generics);
    }

    public override string ToString() {
        if (bits == null) {
            return name;
        } else {
            return name + bits.ToString();
        }
    }
}
