using System;
using System.Linq;
using System.Collections.Generic;

public class JSONInt : IJSONValue {
    public int? Value;

    public JSONInt(int? value) {
        Value = value;
    }

    public string ToJSON() {
        return Value.HasValue ? Value.ToString() : "null";
    }
}
