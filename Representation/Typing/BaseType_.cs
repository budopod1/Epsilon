using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class BaseType_ : IEquatable<BaseType_> {
    // https://en.wikipedia.org/wiki/Set_(mathematics)#Special_sets_of_numbers_in_mathematics
    public static List<string> BuiltInTypes_ = new List<string> {
        "Void",
        "Bool", // equivalent to W1
        "Byte", // equivalent to W8
        "W", // whole numbers (unsigned ints)
        "Z", // integers (signed ints)
        "Q", // floats
        "Array",
        "File",
        "Optional",
        "Null"
    };

    public static List<string> NumberTypes_ = new List<string> {
        "Byte", "W", "Z", "Q", "Bool"
    };

    public static Dictionary<string, int> BitTypes_ = new Dictionary<string, int> {
        {"W", 32}, {"Z", 32}, {"Q", 64}
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

    public static Dictionary<string, int> GenericsAmounts = new Dictionary<string, int> {
        {"Array", 1},
        {"Optional", 1}
    };

    public static Dictionary<string, List<string>> ConvertibleTo = new Dictionary<string, List<string>> {
        {"Bool", new List<string> {"Byte", "W", "Z", "Q"}},
        {"Byte", new List<string> {"W", "Z", "Q"}},
        {"W", new List<string> {"Byte", "Z", "Q"}},
        {"Z", new List<string> {"Q"}},
        {"Optional", new List<string> {"Bool"}},
    };

    public static Dictionary<string, List<string>> EquivalentToBesidesBits = new Dictionary<string, List<string>> {
        {"Bool", new List<string> {"W", "Z"}},
        {"W", new List<string> {"Bool", "Z", "Byte"}},
        {"Byte", new List<string> {"W", "Z"}},
    };

    public static Dictionary<string, List<string>> CastableTo = new Dictionary<string, List<string>> {
        {"Bool", new List<string> {"Byte", "W", "Z", "Q"}},
        {"Byte", new List<string> {"Bool", "W", "Z", "Q"}},
        {"W", new List<string> {"Bool", "Byte", "Z", "Q"}},
        {"Z", new List<string> {"Bool", "Byte", "W", "Q"}},
        {"Q", new List<string> {"Bool", "Byte", "W", "Z"}},
    };

    public static List<string> Optionable = new List<string> {
        "Array"
    };

    public static List<string> ValueTypes_ = new List<string> {
        "Bool", "Byte", "W", "Z", "Q", "Null"
    };

    string name;
    int? bits;

    public BaseType_(string name, int? bits = null) {
        if (BitTypes_.ContainsKey(name)) {
            if (bits == null) bits = BitTypes_[name];
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

    public int? GetBitsOrDefault() {
        int defaultBits = 0;
        if (SpecialDefaultBits.ContainsKey(name)) {
            defaultBits = SpecialDefaultBits[name];
        } else if (BitTypes_.ContainsKey(name)) {
            defaultBits = BitTypes_[name];
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
        if (IsAny() && !other.IsVoid()) return true;
        if (other.IsAny() && !IsVoid()) return true;
        return name == other.GetName() && bits == other.GetBits();
    }

    public bool IsConvertibleTo(BaseType_ other) {
        string oName = other.GetName();
        if (IsAny() && !other.IsVoid()) 
            return true;
        if (other.IsAny() && !IsVoid()) 
            return true;
        if (name == oName) return true;
        if (ConvertibleTo.ContainsKey(name))
            return ConvertibleTo[name].Contains(oName);
        return false;
    }

    public bool IsEquivalentTo(BaseType_ other) {
        if (GetBitsOrDefault() != other.GetBitsOrDefault()) return false;
        string oName = other.GetName();
        if (EquivalentToBesidesBits.ContainsKey(name)) {
            return EquivalentToBesidesBits[name].Contains(oName);
        } else {
            return name == oName;
        }
    }

    public bool IsCastableTo(BaseType_ other) {
        string oName = other.GetName();
        if (name == oName) return true;
        if (IsAny() && !other.IsVoid()) 
            return true;
        if (other.IsAny() && !IsVoid()) 
            return true;
        if (CastableTo.ContainsKey(name))
            return CastableTo[name].Contains(oName);
        return false;
    }

    public bool IsValue() {
        return BuiltInTypes_.Contains(name);
    }

    public bool IsAny() {
        return name == "Any";
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

    public int GenericsAmount() {
        if (GenericsAmounts.ContainsKey(name)) {
            return GenericsAmounts[name];
        } else {
            return 0;
        }
    }

    public bool IsBuiltin() {
        return BuiltInTypes_.Contains(name);
    }

    public bool IsOptionable() {
        return Optionable.Contains(name) || !IsBuiltin();
    }

    public bool IsNullable() {
        return name == "Optional";
    }

    public bool IsZeroInitializable() {
        return IsNullable() || IsNumber();
    }

    public bool IsValueType_() {
        return ValueTypes_.Contains(name);
    }

    public override string ToString() {
        if (bits == null) {
            return name;
        } else {
            return name + bits.ToString();
        }
    }
}
