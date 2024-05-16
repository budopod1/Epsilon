using System;
using System.Collections.Generic;

public class SPECType_Creator {
    List<Type_> types_ = new List<Type_>();
    
    public string MakeSPECType_(Type_ type_) {
        types_.Add(type_);
        return type_.ToString();
    }

    public JSONList GetJSON() {
        JSONList list = new JSONList();
        HashSet<string> appeared = new HashSet<string>();
        Queue<Type_> queue = new Queue<Type_>(types_);
        while (queue.Count > 0) {
            Type_ type_ = queue.Dequeue();
            string text = type_.ToString();
            if (appeared.Contains(text)) continue;
            bool requirementsMet = true;
            JSONList generics = new JSONList();
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
                JSONObject tobj = new JSONObject();
                tobj["given_name"] = new JSONString(text);
                BaseType_ baseType_ = type_.GetBaseType_();
                tobj["name"] = new JSONString(baseType_.GetName());
                tobj["bits"] = JSONInt.OrNull(baseType_.GetBits());
                tobj["generics"] = generics;
                list.Add(tobj);
                appeared.Add(text);
            } else {
                queue.Enqueue(type_);
            }
        }
        return list;
    }
}
