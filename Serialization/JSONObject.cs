using System;
using System.Linq;
using System.Collections.Generic;

public class JSONObject : Dictionary<string, IJSONValue>, IJSONValue {
    public string ToJSON() {
        return "{"+String.Join(", ", 
            this.Select(pair=>Utils.EscapeStringToLiteral(pair.Key)
            + ": " + pair.Value.ToJSON())
        )+"}";
    }
}
