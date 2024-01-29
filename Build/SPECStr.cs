using System;
using System.Linq;

public class SPECStr : ISPECVal {
    public string Value;

    public SPECStr(string value) {
        Value = value;
    }

    public override string ToString() {
        return "\"" + Value;
    }
}
