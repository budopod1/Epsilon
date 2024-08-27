public class InvalidSPECResourceException(ShapedJSON obj, string epslspec, string source) : Exception {
    readonly ShapedJSON obj = obj;
    readonly string epslspec = epslspec;
    readonly string source = source;

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
