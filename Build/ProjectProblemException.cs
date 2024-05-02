using System;

public class ProjectProblemException : Exception {
    public string Problem;

    public ProjectProblemException(string problem) {
        Problem = problem;
    }
}
