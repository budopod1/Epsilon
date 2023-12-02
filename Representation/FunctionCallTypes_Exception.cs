using System;

public class FunctionCallTypes_Exception : Exception {
    public int ArgumentIndex;
    
    public FunctionCallTypes_Exception(string message, int argumentIndex) : base(message) {
        ArgumentIndex = argumentIndex;
    }
}
