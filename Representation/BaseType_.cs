using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class BaseType_ : IEquatable<BaseType_> {
    // https://en.wikipedia.org/wiki/
    // Set_(mathematics)#Special_sets_of_numbers_in_mathematics
    public static List<string> BuiltInTypes_ = new List<string> {
        "Void",
        "Bool",
        "Byte", // equivalent to W8
        "W", // whole numbers (unsigned ints)
        "Z", // integers (signed ints)
        "Q", // floats
        "Array",
        "Struct",
    };

    public static List<string> SpecialTypes_ = new List<string> {
        "Any", // matches any type_ except Unkown and Void
        "Unknown",
    };

    public static List<string> NumberTypes_ = new List<string> {
        "Byte", "W", "Z", "Q", "Bool"
    };

    public static List<string> BitTypes_ = new List<string> {
        "W", "Z", "Q"
    };

    public static List<string> IntTypes_ = new List<string> {
        "W", "Z", "Bool", "Byte"
    };

    public static List<string> FloatTypes_ = new List<string> {
        "Q"
    };

    public static List<string> BitMeaningfulTypes_ = new List<string> {
        "W", "Z", "Q", "Bool", "Byte"
    };

    public static Dictionary<string, int> SpecialDefaultBits = new Dictionary<string, int> {
        {"Bool", 1},
        {"Byte", 8}
    };

    public static List<string> GenericsTypes_ = new List<string> {
        "Array"
    };

    public static Dictionary<string, List<string>> ConvertibleTo = new Dictionary<string, List<string>> {
        {"Bool", new List<string> {"Byte", "W", "Z", "Q"}},
        {"Byte", new List<string> {"W", "Z", "Q"}},
        {"W", new List<string> {"Byte", "Z", "Q"}},
        {"Z", new List<string> {"Q"}},
    };

    public static Dictionary<string, List<string>> CastableTo = new Dictionary<string, List<string>> {
        {"Bool", new List<string> {"Byte", "W", "Z", "Q"}},
        {"Byte", new List<string> {"Bool", "W", "Z", "Q"}},
        {"W", new List<string> {"Bool", "Byte", "Z", "Q"}},
        {"Z", new List<string> {"Bool", "Byte", "W", "Q"}},
        {"Q", new List<string> {"Bool", "Byte", "W", "Z"}},
    };

    public static List<string> BitsImportant = new List<string> {
        "W", "Z"
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
            throw new BaseType_BitsException(
                $"You can't set the bits for {name}"
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

    public int GetBitsOrDefault() {
        int defaultBits = DefaultBits;
        if (SpecialDefaultBits.ContainsKey(name)) {
            defaultBits = SpecialDefaultBits[name];
        }
        return bits.GetValueOrDefault(defaultBits);
    }

    public int? GetBitsOrDefaultIfMeaningful() {
        if (BitMeaningfulTypes_.Contains(name)) {
            return GetBitsOrDefault();
        } else {
            return null;
        }
    }

    public bool IsNumber() {
        return NumberTypes_.Contains(name);
    }

    public bool Equals(BaseType_ other) {
        if (IsAny() && !other.IsNon()) return true;
        if (other.IsAny() && !IsNon()) return true;
        return name == other.GetName() && bits == other.GetBits();
    }

    public bool IsConvertibleTo(BaseType_ other) {
        string oName = other.GetName();
        if (IsAny() && !other.IsNon()) 
            return true;
        if (other.IsAny() && !IsNon()) 
            return true;
        if (BitsImportant.Contains(name) && BitsImportant.Contains(oName)) {
            if (other.GetBitsOrDefault() < GetBitsOrDefault())
                return false;
        }
        if (name == oName) return true;
        if (ConvertibleTo.ContainsKey(name))
            return ConvertibleTo[name].Contains(oName);
        return false;
    }

    public bool IsCastableTo(BaseType_ other) {
        string oName = other.GetName();
        if (name == oName) return true;
        if (IsAny() && !other.IsNon()) 
            return true;
        if (other.IsAny() && !IsNon()) 
            return true;
        if (CastableTo.ContainsKey(name))
            return CastableTo[name].Contains(oName);
        return false;
    }

    public bool IsAny() {
        return name == "Any";
    }

    public bool IsNon() {
        return name == "Unkown" || name == "Void";
    }

    public bool IsVoid() {
        return name == "Void";
    }

    public bool IsInt() {
        return IntTypes_.Contains(name);
    }

    public bool IsFloat() {
        return FloatTypes_.Contains(name);
    }

    public override string ToString() {
        if (bits == null) {
            return name;
        } else {
            return name + bits.ToString();
        }
    }
}
