using System;
using System.Linq;

public class JSONString : IJSONValue {
    public string Value;

    public JSONString(string value) {
        Value = value;
    }

    public string ToJSON() {
        if (Value == null) {
            return "null";
        } else {
            return Utils.EscapeStringToLiteral(Value);
        }
    }
}
