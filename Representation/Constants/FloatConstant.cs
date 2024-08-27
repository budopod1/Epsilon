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
        throw new InvalidOperationException("Float constants are currently not implemented");
        /*
        IJSONValue valJSON;
        if (double.IsNaN(value)) {
            valJSON = new JSONString("NaN");
        } else if (double.IsPositiveInfinity(value)) {
            valJSON = new JSONString("+Infinity");
        } else if (double.IsNegativeInfinity(value)) {
            valJSON = new JSONString("-Infinity");
        } else {
            valJSON = new JSONDouble(value);
        }
        return new JSONObject() {
            ["type"] = new JSONString("float"),
            ["value"] = valJSON
        };
        */
    }
}
