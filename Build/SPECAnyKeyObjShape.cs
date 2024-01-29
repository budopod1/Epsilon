using System;
using System.Collections.Generic;

public class SPECAnyKeyObjShape : ISPECShape {
    ISPECShape subShape;

    public SPECAnyKeyObjShape(ISPECShape subShape) {
        this.subShape = subShape;
    }

    public bool Matches(ISPECVal val) {
        SPECObj obj = val as SPECObj;
        if (obj == null) return false;
        foreach (ISPECVal sub in obj.Values) {
            if (!subShape.Matches(sub)) return false;
        }
        return true;
    }
}
