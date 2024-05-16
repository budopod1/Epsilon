using System;
using System.Globalization;

public class CharConstant : IIntConstant {
    byte value;

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
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("int");
        obj["value"] = new JSONInt(value);
        return obj;
    }
}
