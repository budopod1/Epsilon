namespace Epsilon;
public class BaseType_ : IEquatable<BaseType_> {
    // https://en.wikipedia.org/wiki/Set_(mathematics)#Special_sets_of_numbers_in_mathematics
    public static readonly List<string> BuiltInTypes_ = [
        "Bool", // equivalent to W1
        "Byte", // equivalent to W8
        "W", // whole numbers (unsigned ints)
        "Z", // integers (signed ints)
        "Q", // floats
        "Array",
        "Optional",
        "Null",
        "Internal",
        "Poly"
    ];

    public static readonly List<string> NumberTypes_ = [
        "Byte", "W", "Z", "Q", "Bool"
    ];

    public static readonly Dictionary<string, int> BitTypes_ = new() {
        {"W", 32}, {"Z", 32}, {"Q", 64}
    };

    public static readonly List<string> IntTypes_ = [
        "W", "Z", "Bool", "Byte"
    ];

    public static readonly List<string> FloatTypes_ = [
        "Q"
    ];

    public static readonly List<string> BitMeaningfulTypes_ = [
        "W", "Z", "Q", "Bool", "Byte"
    ];

    public static readonly Dictionary<string, int> SpecialPresetBits = new() {
        {"Bool", 1},
        {"Byte", 8}
    };

    public static readonly Dictionary<string, int> GenericsAmounts = new() {
        {"Array", 1},
        {"Optional", 1},
        {"Poly", 1}
    };

    public static readonly Dictionary<string, List<string>> ConvertibleTo = new() {
        {"Bool", new List<string> {"Byte", "W", "Z", "Q"}},
        {"Byte", new List<string> {"W", "Z", "Q"}},
        {"W", new List<string> {"Z", "Q"}},
        {"Z", new List<string> {"Q"}},
    };

    public static readonly Dictionary<string, List<string>> EquivalentToBesidesBits = new() {
        {"Bool", new List<string> {"W", "Z"}},
        {"W", new List<string> {"Bool", "Z", "Byte"}},
        {"Byte", new List<string> {"W", "Z"}},
    };

    public static readonly Dictionary<string, List<string>> CastableTo = new() {
        {"Bool", new List<string> {"Byte", "W", "Z", "Q"}},
        {"Byte", new List<string> {"Bool", "W", "Z", "Q"}},
        {"W", new List<string> {"Bool", "Byte", "Z", "Q"}},
        {"Z", new List<string> {"Bool", "Byte", "W", "Q"}},
        {"Q", new List<string> {"Bool", "Byte", "W", "Z"}},
    };

    public static readonly List<string> OptionableNonValue = [
        "Array", "Poly"
    ];

    public static readonly List<string> ValueTypes_ = [
        "Bool", "Byte", "W", "Z", "Q", "Null", "Internal"
    ];

    readonly string name;
    readonly int? bits;

    public BaseType_(string name, int? bits = null) {
        if (BitTypes_.TryGetValue(name, out int value)) {
            bits ??= value;
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
        if (SpecialPresetBits.TryGetValue(name, out int preset)) {
            defaultBits = preset;
        } else if (BitTypes_.TryGetValue(name, out int bits)) {
            defaultBits = bits;
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
        return name == other.GetName()
            && GetBitsOrDefaultIfMeaningful() == other.GetBitsOrDefaultIfMeaningful();
    }

    public bool Matches(BaseType_ other) {
        if (IsAny() || other.IsAny()) return true;
        return Equals(other);
    }

    public bool IsConvertibleTo(BaseType_ other) {
        string oName = other.GetName();
        if (IsAny() || other.IsAny()) return true;
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
        if (IsAny() || other.IsAny()) return true;
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

    public bool IsNull() {
        return name == "Null";
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
        return OptionableNonValue.Contains(name) || NumberTypes_.Contains(name)
            || !IsBuiltin();
    }

    public bool IsNullable() {
        return name == "Optional" || IsNull();
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
