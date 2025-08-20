using CsJSONTools;
using System.Globalization;

namespace Epsilon;
public class FloatConstant(double value) : INumberConstant {
    readonly double value = value;

    public static FloatConstant FromString(string value) {
        return new FloatConstant(double.Parse(
            value, CultureInfo.InvariantCulture
        ));
    }

    public Type_ GetType_() {
        return new Type_("R");
    }

    public double GetDoubleValue() {
        return value;
    }

    public bool IsTruthy() {
        return value != 0;
    }

    public IJSONValue GetJSON() {
        string type;
        IJSONValue valJSON;
        if (double.IsNaN(value)) {
            type = "float-special";
            valJSON = new JSONString("NaN");
        } else if (double.IsPositiveInfinity(value)) {
            type = "float-special";
            valJSON = new JSONString("+Infinity");
        } else if (double.IsNegativeInfinity(value)) {
            type = "float-special";
            valJSON = new JSONString("-Infinity");
        } else {
            type = "float-standard";
            valJSON = new JSONDouble(value);
        }
        return new JSONObject() {
            ["type"] = new JSONString(type),
            ["value"] = valJSON
        };
    }
}
