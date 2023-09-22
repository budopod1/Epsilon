using System;
using System.Globalization;

public class FloatConstant : IConstant {
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
}
