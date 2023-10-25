using System;
using System.Globalization;

public class UnsignedIntConstant : IConstant {
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
}
