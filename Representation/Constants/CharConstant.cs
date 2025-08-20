using CsJSONTools;

namespace Epsilon;
public class CharConstant : IIntConstant {
    readonly byte value;

    public CharConstant(byte value) {
        this.value = value;
    }

    public CharConstant(char value) {
        this.value = (byte)value;
    }

    public static CharConstant FromString(string value) {
        string parsed = JSONTools.FromLiteral(value);
        if (parsed.Length != 1) {
            throw new ArgumentException(
                "Character constants must contain exactly one byte"
            );
        };
        return new CharConstant(parsed[0]);
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
        return value != '\0';
    }

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["type"] = new JSONString("int"),
            ["value"] = new JSONInt(value)
        };
    }
}
