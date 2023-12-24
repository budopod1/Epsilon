using System;
using System.Globalization;

public class CharConstant : INumberConstant, IIntConstant {
    byte value;

    public CharConstant(byte value) {
        this.value = value;
    }

    public CharConstant(char value) {
        this.value = Convert.ToByte(value);
    }

    public static CharConstant FromString(string value) {
        if (value[1] == '\\') {
            char chr = value[2];
            if (chr == '\'') return new CharConstant('\'');
            return new CharConstant(Utils.UnescapeReplacements[chr]);
        } else {
            return new CharConstant(value[1]);
        }
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

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("int");
        obj["value"] = new JSONInt(value);
        return obj;
    }
}
