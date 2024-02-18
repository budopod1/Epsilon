using System;

public class CommandFailureException : Exception {
    public CommandFailureException(string message) : base(message) {}
}
