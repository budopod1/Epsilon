using System;
using System.Collections.Generic;

public class SPECObjShape : ISPECShape {
    Dictionary<string, ISPECShape> shape;
    
    public SPECObjShape(Dictionary<string, ISPECShape> shape) {
        this.shape = shape;
    }

    public bool Matches(ISPECVal val) {
        SPECObj obj = val as SPECObj;
        if (obj == null) return false;
        foreach (KeyValuePair<string, ISPECShape> pair in shape) {
            if (!obj.ContainsKey(pair.Key)) return false;
            if (!pair.Value.Matches(obj[pair.Key])) return false;
        }
        return true;
    }
}
