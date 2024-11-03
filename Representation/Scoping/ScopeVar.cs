using CsJSONTools;

namespace Epsilon;
public class ScopeVar(string name, Type_ type_) {
    readonly string name = name;
    readonly Type_ type_ = type_;

    public string GetName() {
        return name;
    }

    public Type_ GetType_() {
        return type_;
    }

    public IJSONValue GetJSON(int id) {
        return new JSONObject {
            ["name"] = new JSONString(name),
            ["type_"] = type_.GetJSON(),
            ["id"] = new JSONInt(id)
        };
    }
}
