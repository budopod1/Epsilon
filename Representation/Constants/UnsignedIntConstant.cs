using System.Globalization;

public class UnsignedIntConstant(uint value) : INumberConstant, IIntConstant {
    readonly uint value = value;

    public static UnsignedIntConstant FromString(string value) {
        return new UnsignedIntConstant(uint.Parse(
            value, CultureInfo.InvariantCulture
        ));
    }

    public Type_ GetType_() {
        return new Type_("W");
    }

    public double GetDoubleValue() {
        return value;
    }

    public int GetIntValue() {
        return (int)value;
    }

    public bool IsTruthy() {
        return value != 0;
    }

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["type"] = new JSONString("uint"),
            ["value"] = new JSONInt((int)value)
        };
    }
}
