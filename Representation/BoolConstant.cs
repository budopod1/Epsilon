using System;
using System.Collections.Generic;

public class BoolConstant : INumberConstant, IIntConstant {
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

    public int GetIntValue() {
        return value ? 1 : 0;
    }

    public double GetDoubleValue() {
        return GetIntValue();
    }

    public bool IsTruthy() {
        return value;
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("bool");
        obj["value"] = new JSONBool(value);
        return obj;
    }
}
