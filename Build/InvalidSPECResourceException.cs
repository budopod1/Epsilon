using System;
using System.IO;

public class InvalidSPECResourceException : Exception {
    ShapedJSON obj;
    string epslspec;
    string source;
    
    public InvalidSPECResourceException(ShapedJSON obj, string epslspec, string source) {
        this.obj = obj;
        this.epslspec = epslspec;
        this.source = source;
    }

    public ShapedJSON GetObj() {
        return obj;
    }

    public string GetEPSLSPEC() {
        return epslspec;
    }

    public string GetSource() {
        return source;
    }
}
