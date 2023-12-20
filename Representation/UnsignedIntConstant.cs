using System;
using System.Globalization;

public class UnsignedIntConstant : INumberConstant {
    uint value;

    public UnsignedIntConstant(uint value) {
        this.value = value;
    }

    public static UnsignedIntConstant FromString(string value) {
        return new UnsignedIntConstant(UInt32.Parse(
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

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("uint");
        obj["value"] = new JSONInt((int)value);
        return obj;
    }
}
