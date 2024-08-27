public class BoolConstant(bool value) : INumberConstant, IIntConstant {
    readonly bool value = value;

    public static BoolConstant FromString(string value) {
        if (value == "true") {
            return new BoolConstant(true);
        } else if (value == "false") {
            return new BoolConstant(false);
        } else {
            throw new InvalidOperationException(
                $"Cannot convert '{value}' to bool"
            );
        }
    }

    public Type_ GetType_() {
        return new Type_("Bool");
    }

    public int GetIntValue() {
        return value ? 1 : 0;
    }

    public double GetDoubleValue() {
        return GetIntValue();
    }

    public bool IsTruthy() {
        return value;
    }

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["type"] = new JSONString("bool"),
            ["value"] = new JSONBool(value)
        };
    }
}
