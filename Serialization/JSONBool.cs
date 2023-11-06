using System;
using System.Linq;
using System.Collections.Generic;

public class JSONBool : IJSONValue {
    public bool? Value;

    public JSONBool(bool? value) {
        Value = value;
    }

    public string ToJSON() {
        return Value.HasValue ? (Value.Value?"true":"false") : "null";
    }
}
