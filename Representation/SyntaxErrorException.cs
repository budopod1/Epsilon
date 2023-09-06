using System;

public class SyntaxErrorException : Exception {
    public SyntaxErrorException(string message) : base(message) {}
}
