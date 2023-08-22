using System;
using System.Globalization;

public class IntConstant : IConstant {
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
}
