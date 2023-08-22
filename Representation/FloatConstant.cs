using System;
using System.Globalization;

public class FloatConstant : IConstant {
    float value;

    public FloatConstant(float value) {
        this.value = value;
    }

    public static FloatConstant FromString(string value) {
        return new FloatConstant(float.Parse(
            value, CultureInfo.InvariantCulture
        ));
    }

    public Type_ GetType_() {
        return new Type_("Q");
    }
}
