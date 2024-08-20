using System;
using System.Globalization;

public class FloatConstant(double value) : INumberConstant {
    readonly double value = value;

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

    public bool IsTruthy() {
        return value != 0;
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new() {
            ["type"] = new JSONString("float")
        };
        IJSONValue json;
        if (double.IsNaN(value)) {
            json = new JSONString("NaN");
        } else if (double.IsPositiveInfinity(value)) {
            json = new JSONString("+Infinity");
        } else if (double.IsNegativeInfinity(value)) {
            json = new JSONString("-Infinity");
        } else {
            json = new JSONDouble(value);
        }
        obj["value"] = json;
        return obj;
    }
}
