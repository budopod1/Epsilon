using System.Globalization;

public class ByteConstant(byte value) : IIntConstant {
    readonly byte value = value;

    public static ByteConstant FromString(string value) {
        return new ByteConstant(byte.Parse(
            value, CultureInfo.InvariantCulture
        ));
    }

    public Type_ GetType_() {
        return new Type_("Byte");
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
