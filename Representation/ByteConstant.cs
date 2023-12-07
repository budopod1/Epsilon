using System;
using System.Globalization;

public class ByteConstant : INumberConstant {
    byte value;

    public ByteConstant(byte value) {
        this.value = value;
    }

    public static ByteConstant FromString(string value) {
        return new ByteConstant(Byte.Parse(
            value, CultureInfo.InvariantCulture
        ));
    }

    public Type_ GetType_() {
        return new Type_("Byte");
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
