using System;
using System.Linq;
using System.Collections.Generic;

public static class StructsCtx {
    static Dictionary<string, Struct> structs = new Dictionary<string, Struct>();
    static bool structsLoaded = false;
    
    public static Struct GetStructFromID(string id) {
        if (structs.ContainsKey(id)) {
            return structs[id];
        } else {
            return null;
        }
    }

    public static Struct GetStructFromType_(Type_ type_) {
        string id = type_.GetBaseType_().GetName();
        if (structs.ContainsKey(id)) {
            return structs[id];
        } else {
            return null;
        }
    }

    public static Struct GetStructOrPolyFromType_(Type_ type_) {
        if (type_.GetBaseType_().GetName() == "Poly") {
            type_ = type_.GetGeneric(0);
        }
        string name = type_.GetBaseType_().GetName();
        if (structs.ContainsKey(name)) {
            return structs[name];
        } else {
            return null;
        }
    }

    public static void Add(Struct struct_) {
        string id = struct_.GetID();
        if (structs.ContainsKey(id)) {
            Struct other = structs[id];
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
        return structs.Values.ToHashSet();
    }
}