using System.Globalization;

public class IntConstant(int value) : IIntConstant {
    readonly int value = value;

    public static IntConstant FromString(string value) {
        return new IntConstant(int.Parse(
            value, CultureInfo.InvariantCulture
        ));
    }

    public Type_ GetType_() {
        return new Type_("Z");
    }

    public int GetIntValue() {
        return value;
    }

    public double GetDoubleValue() {
        return value;
    }

    public bool IsTruthy() {
        return value != 0;
    }

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["type"] = new JSONString("int"),
            ["value"] = new JSONInt(value)
        };
    }
}
