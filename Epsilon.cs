using System;
using System.IO;
using System.Collections.Generic;

public class Epsilon {
    public static int Main(string[] args) {
        ArgumentParser parser = new ArgumentParser(
            "mono Epsilon.exe",
            "A compiler for the Epsilon programming language"
        );

        bool alwaysProj = false;
        bool neverProj = false;

        parser.AddOption(() => {alwaysProj = true;}, "Always use project mode", 
            "p", "project-mode");
        parser.AddOption(() => {neverProj = true;}, "Never use project mode", 
            "P", "no-project-mode");
        
        parser.Expect(new KeywordExpectation("compile", false));
        InputExpectation inputFile = parser.Expect(new InputExpectation("input file"));
        InputExpectation outputFile = parser.Expect(new InputExpectation("output file"));

        PossibilitiesExpectation outputType = parser.AddOption(
            new PossibilitiesExpectation("executable", "llvm-ll", "llvm-bc"), 
            "The output file type", "t", "output-type"
        );
        
        parser.Parse(args);

        if (alwaysProj && neverProj) {
            parser.DisplayProblem("The 'project-mode' and 'no-project-mode' options are mutually exclusive");
        }
        
        Builder builder = new Builder();

        builder.ALWAYS_PROJECT = alwaysProj;
        builder.NEVER_PROJECT = neverProj;
        
        CompilationResult result = builder.Build(inputFile.Matched);
        if (result.GetStatus() == CompilationResultStatus.GOOD 
            && outputType.Value() == "executable") {
            result = builder.ToExecutable();
        }
        if (result.GetStatus() != CompilationResultStatus.GOOD) {
            if (result.HasMessage()) parser.DisplayProblem(result.GetMessage());
            return 1;
        }

        string sourceFile;
        
        switch (outputType.Value()) {
            case "llvm-bc":
                sourceFile = "code-linked.bc";
                break;
            case "llvm-ll":
                Utils.RunCommand("llvm-dis", new List<string> {
                    "--", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-linked.bc")
                });
                sourceFile = "code-linked.ll";
                break;
            case "executable":
                sourceFile = "code";
                break;
            default:
                throw new InvalidOperationException();
        }
        
        try {
            string executable = Utils.JoinPaths(Utils.ProjectAbsolutePath(), sourceFile);
            File.Copy(executable, outputFile.Matched, true);
        } catch (IOException) {
            parser.DisplayProblem("Could not write specified output file");
            return 1;
        }
        
        return 0;
    }
}
