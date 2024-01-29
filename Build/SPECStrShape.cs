using System;

public class SPECStrShape : ISPECShape {
    public bool Matches(ISPECVal val) {
        return val is SPECStr;
    }
}
