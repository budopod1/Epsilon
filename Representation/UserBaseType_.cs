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
        "Str"
    };

    public static UserBaseType_ ParseString(string content, List<string> structNames) {
        if (structNames.Contains(content) || BaseType_.BuiltInTypes_.Contains(content)
            || SpecialFullBaseType_Names.Contains(content)) {
            return new UserBaseType_(content);
        }
        System.Text.RegularExpressions.Match match = Regex.Match(
            content, $@"({String.Join('|', BaseType_.BitTypes_)})(\d+)"
        );
        if (match.Success) {
            string name = match.Groups[1].Value;
            if (BaseType_.BitTypes_.Contains(name)) {
                int bits = Int32.Parse(match.Groups[2].Value);
                return new UserBaseType_(name, bits);
            }
        }
        return null;
    }

    static Dictionary<string, Type_> SpecialFullBaseTypes_ = new Dictionary<string, Type_> {
        {"Str", new Type_("Array", new List<Type_> {new Type_("Byte")})}
    };

    public Type_ ToType_(List<Type_> generics) {
        if (SpecialFullBaseType_Names.Contains(name)) {
            // TODO: add check that generics must be empty
            return SpecialFullBaseTypes_[name];
        }
        return new Type_(new BaseType_(name, bits), generics);
    }
}
