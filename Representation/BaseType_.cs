using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class BaseType_ : IEquatable<BaseType_> {
    // https://en.wikipedia.org/wiki/
    // Set_(mathematics)#Special_sets_of_numbers_in_mathematics
    public static List<string> BuiltInTypes_ = new List<string> {
        "Unknown",
        "Void",
        "Bool",
        "Byte", // equivalent to W8
        "W", // whole numbers (unsigned ints)
        "Z", // integers (signed ints)
        "Q", // floats
        "Array",
        "Struct",
    };

    public static List<string> NumberTypes_ = new List<string> {
        "Byte", "W", "Z", "Q"
    };

    public static List<string> BitTypes_ = new List<string> {
        "W", "Z", "Q"
    };

    public static List<string> GenericsTypes_ = new List<string> {
        "Array"
    };

    public static Dictionary<string, List<string>> ConvertibleTo = new Dictionary<string, List<string>> {
        {"Byte", new List<string> {"W", "Z", "Q"}},
        {"W", new List<string> {"Byte", "Z", "Q"}},
        {"Z", new List<string> {"Q"}},
    };

    public static int DefaultBits = 32;

    string name;
    int? bits;

    public static BaseType_ ParseString(string source,
                                        List<string> type_Names) {
        if (type_Names.Contains(source) || BuiltInTypes_.Contains(source)) {
            return new BaseType_(source);
        }
        System.Text.RegularExpressions.Match match = Regex.Match(
            source, $@"({String.Join('|', BitTypes_)})(\d+)"
        );
        if (match.Success) {
            string name = match.Groups[1].Value;
            if (BitTypes_.Contains(name)) {
                int bits = Int32.Parse(match.Groups[2].Value);
                return new BaseType_(name, bits);
            }
        }
        return null;
    }

    public BaseType_(string name, int? bits = null) {
        if (BitTypes_.Contains(name)) {
            if (bits == null) bits = DefaultBits;
        } else if (bits != null) {
            throw new ArgumentException(
                $"BaseType_ {name} bits must be null"
            );
        }
        this.name = name;
        this.bits = bits;
    }

    public string GetName() {
        return name;
    }

    public int? GetBits() {
        return bits;
    }

    public bool IsNumber() {
        return NumberTypes_.Contains(name);
    }

    public bool Equals(BaseType_ other) {
        return name == other.GetName() && bits == other.GetBits();
    }

    public bool IsConvertibleTo(BaseType_ other) {
        string oName = other.GetName();
        if (name == oName) return true;
        if (ConvertibleTo.ContainsKey(name)) {
            return ConvertibleTo[name].Contains(oName);
        }
        return false;
    }

    public override string ToString() {
        if (bits == null) {
            return name;
        } else {
            return name + bits.ToString();
        }
    }
}
