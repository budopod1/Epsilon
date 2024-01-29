using System;
using System.Collections.Generic;

public class SPECListShape : ISPECShape {
    ISPECShape subShape;

    public SPECListShape(ISPECShape subShape) {
        this.subShape = subShape;
    }

    public bool Matches(ISPECVal val) {
        SPECList list = val as SPECList;
        if (list == null) return false;
        foreach (ISPECVal sub in list) {
            if (!subShape.Matches(sub)) return false;
        }
        return true;
    }
}
