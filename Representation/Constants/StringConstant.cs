using CsJSONTools;

namespace Epsilon;
public class StringConstant(string value) : IConstant {
    readonly string value = value;

    public static StringConstant FromString(string value) {
        return new StringConstant(JSONTools.FromLiteral(value));
    }

    public Type_ GetType_() {
        return Type_.String();
    }

    public string GetValue() {
        return value;
    }

    public bool IsTruthy() {
        return value.Length > 0;
    }

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["type"] = new JSONString("string"),
            ["value"] = new JSONString(value)
        };
    }
}
