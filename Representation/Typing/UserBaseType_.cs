namespace Epsilon;
public class UserBaseType_(string name, int? bits = null) {
    readonly string name = name;
    readonly int? bits = bits;
    public readonly static List<string> SpecialFullBaseType_Names = [
        "Str", "L"
    ];

    public static UserBaseType_ ParseString(string content, HashSet<LocatedID> structIds) {
        foreach (LocatedID structId in structIds) {
            if (content == structId.Name) return new UserBaseType_(structId.GetID());
        }

        if (BaseType_.BuiltInTypes_.Contains(content)
            || SpecialFullBaseType_Names.Contains(content)) {
            return new UserBaseType_(content);
        }

        foreach (string name in BaseType_.BitTypes_.Keys) {
            if (!content.StartsWith(name)) continue;
            string rest = content[name.Length..];
            try {
                int bits = int.Parse(rest);
                return new UserBaseType_(name, bits);
            } catch (FormatException) {
                continue;
            }
        }

        return null;
    }

    static readonly Dictionary<string, Type_> SpecialFullBaseTypes_ = new() {
        {"Str", Type_.String()}, {"L", new Type_("W", 64)}
    };

    public Type_ ToType_() {
        return ToType_([]);
    }

    public Type_ ToType_(List<Type_> generics) {
        if (SpecialFullBaseType_Names.Contains(name)) {
            if (generics.Count > 0) {
                throw new IllegalType_Exception(
                    $"Alias type {name} cannot have generics"
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
