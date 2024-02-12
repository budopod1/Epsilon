using System;

public class CompilationResult {
    CompilationResultStatus status;
    string message;

    public CompilationResult(CompilationResultStatus status, string message) {
        this.status = status;
        this.message = message;
    }

    public CompilationResult(CompilationResultStatus status) {
        this.status = status;
        message = null;
    }

    public CompilationResultStatus GetStatus() {
        return status;
    }

    public bool HasMessage() {
        return message != null;
    }

    public string GetMessage() {
        return message;
    }
}
