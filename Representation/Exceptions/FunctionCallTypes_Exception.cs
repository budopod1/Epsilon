using System;

public class FunctionCallTypes_Exception(string message, int argumentIndex) : Exception(message) {
    public int ArgumentIndex = argumentIndex;
}
