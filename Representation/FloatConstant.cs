using System;
using System.Globalization;

public class FloatConstant : INumberConstant {
    double value;

    public FloatConstant(double value) {
        this.value = value;
    }

    public static FloatConstant FromString(string value) {
        return new FloatConstant(double.Parse(
            value, CultureInfo.InvariantCulture
        ));
    }

    public Type_ GetType_() {
        return new Type_("Q");
    }

    public double GetDoubleValue() {
        return value;
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("float");
        IJSONValue json;
        if (Double.IsNaN(value)) {
            json = new JSONString("NaN");
        } else if (Double.IsPositiveInfinity(value)) {
            json = new JSONString("+Infinity");
        } else if (Double.IsNegativeInfinity(value)) {
            json = new JSONString("-Infinity");
        } else {
            json = new JSONDouble(value);
        }
        obj["value"] = json;
        return obj;
    }
}
