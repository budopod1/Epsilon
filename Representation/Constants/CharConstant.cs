using CsJSONTools;

public class CharConstant : IIntConstant {
    readonly byte value;

    public CharConstant(byte value) {
        this.value = value;
    }

    public CharConstant(char value) {
        if (value >= 128) {
            throw new OverflowException("Character constants' codepoints must not exceed 127");
        }
        this.value = Convert.ToByte(value);
    }

    public static CharConstant FromString(string value) {
        return new CharConstant(JSONTools.FromLiteralChar(value));
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
