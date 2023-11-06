using System;
using System.Linq;
using System.Collections.Generic;

public class JSONNull : IJSONValue {
    public string ToJSON() {
        return "null";
    }
}
