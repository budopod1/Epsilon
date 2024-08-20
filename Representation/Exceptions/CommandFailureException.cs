using System;

public class CommandFailureException(string message) : Exception(message) {
}
