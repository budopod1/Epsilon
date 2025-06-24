namespace Epsilon;
public static class StructsCtx {
    static readonly Dictionary<string, Struct> structs = [];
    static bool structsLoaded = false;

    public static Struct GetStructFromID(string id) {
        if (structs.TryGetValue(id, out Struct value)) {
            return value;
        } else {
            return null;
        }
    }

    public static Struct GetStructFromType_(Type_ type_) {
        string id = type_.GetBaseType_().GetName();
        if (structs.TryGetValue(id, out Struct value)) {
            return value;
        } else {
            return null;
        }
    }

    public static Struct GetStructOrPolyFromType_(Type_ type_) {
        if (type_.GetBaseType_().GetName() == "Poly") {
            type_ = type_.GetGeneric(0);
        }
        return GetStructFromType_(type_);
    }

    public static void Add(Struct struct_) {
        string id = struct_.GetID();
        if (structs.TryGetValue(id, out Struct other)) {
            if (!other.Equals(struct_)) {
                throw new ArgumentException(
                    $"Duplicate struct ID '{id}' for different structs"
                );
            }
        } else {
            structs[id] = struct_;
        }
    }

    public static void Extend(IEnumerable<Struct> structs) {
        foreach (Struct struct_ in structs) {
            Add(struct_);
        }
    }

    public static bool AreStructsLoaded() {
        return structsLoaded;
    }

    public static void MarkStructsLoaded() {
        structsLoaded = true;
    }

    public static IEnumerable<Struct> Structs() {
        return structs.Values;
    }

    public static HashSet<Struct> StructSet() {
        return [..structs.Values];
    }
}
