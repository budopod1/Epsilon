public class SPECType_Creator {
    readonly List<Type_> types_ = [];

    public string MakeSPECType_(Type_ type_) {
        types_.Add(type_);
        return type_.ToString();
    }

    public JSONList GetJSON() {
        JSONList list = [];
        HashSet<string> appeared = [];
        Queue<Type_> queue = new(types_);
        while (queue.Count > 0) {
            Type_ type_ = queue.Dequeue();
            string text = type_.ToString();
            if (appeared.Contains(text)) continue;
            bool requirementsMet = true;
            JSONList generics = [];
            foreach (Type_ generic in type_.GetGenerics()) {
                string genericText = generic.ToString();
                if (!appeared.Contains(genericText)) {
                    queue.Enqueue(generic);
                    requirementsMet = false;
                    break;
                }
                generics.Add(new JSONString(genericText));
            }
            if (requirementsMet) {
                BaseType_ baseType_ = type_.GetBaseType_();
                list.Add(new JSONObject {
                    ["given_name"] = new JSONString(text),
                    ["name"] = new JSONString(baseType_.GetName()),
                    ["bits"] = JSONInt.OrNull(baseType_.GetBits()),
                    ["generics"] = generics
                });
                appeared.Add(text);
            } else {
                queue.Enqueue(type_);
            }
        }
        return list;
    }
}
