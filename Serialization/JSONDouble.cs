using System;
using System.Linq;
using System.Collections.Generic;

public class JSONDouble : IJSONValue {
    public double? Value;

    public JSONDouble(double? value) {
        Value = value;
    }

    public string ToJSON() {
        return Value.HasValue ? Value.ToString() : "null";
    }
}
