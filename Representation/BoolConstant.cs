using System;
using System.Collections.Generic;

public class BoolConstant : IConstant {
    bool value;

    public BoolConstant(bool value) {
        this.value = value;
    }

    public static BoolConstant FromString(string value) {
        if (value == "true") {
            return new BoolConstant(true);
        } else if (value == "false") {
            return new BoolConstant(false);
        } else {
            throw new InvalidOperationException(
                $"Cannot convert '{value}' to bool"
            );
        }
    }

    public Type_ GetType_() {
        return new Type_("Bool");
    }
}