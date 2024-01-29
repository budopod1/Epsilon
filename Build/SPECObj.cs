using System;
using System.Linq;
using System.Collections.Generic;

public class SPECObj : Dictionary<string, ISPECVal>, ISPECVal {
    public override string ToString() {
        return "{\n"+Utils.Indent(String.Join("\n", 
            this.Select(pair=>pair.Key + ": " + pair.Value.ToString())
        ))+"\n}";
    }
}
