using System;
using System.Globalization;

public class IntConstant : IIntConstant {
    int value;

    public IntConstant(int value) {
        this.value = value;
    }

    public static IntConstant FromString(string value) {
        return new IntConstant(Int32.Parse(
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

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("int");
        obj["value"] = new JSONInt(value);
        return obj;
    }
}
