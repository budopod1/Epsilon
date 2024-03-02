using System;
using System.IO;
using System.Collections.Generic;

public class Epsilon {
    public static int Main(string[] args) {
        Utils.LoadCommands(new List<string> {
            "bash", "python", "opt", "clang", "python", "llvm-link"
        });
        
        ArgumentParser parser = new ArgumentParser(
            "mono Epsilon.exe",
            "A compiler for the Epsilon programming language"
        );
        
        parser.Expect(new KeywordExpectation("compile", false));
        InputExpectation inputFile = parser.Expect(new InputExpectation("input file"));
        InputExpectation outputFile = parser.Expect(new InputExpectation("output file"));

        /*
        PossibilitiesExpectation outputType = parser.AddOption(
            new PossibilitiesExpectation("executable", "llvm-ir"), 
            "The output file type", "t", "output-type"
        );
        */
        
        parser.Parse(args);
        
        Builder builder = new Builder();
        
        CompilationResult result = builder.Build(inputFile.Matched);
        if (result.GetStatus() == CompilationResultStatus.GOOD) {
            result = builder.ToExecutable();
        }
        if (result.GetStatus() != CompilationResultStatus.GOOD) {
            if (result.HasMessage()) parser.DisplayProblem(result.GetMessage());
            return 1;
        }
        try {
            string executable = Utils.ProjectAbsolutePath()+"/code";
            File.Copy(executable, outputFile.Matched, true);
        } catch (IOException) {
            parser.DisplayProblem("Could not write specified output file");
            return 1;
        }
        return 0;
    }
}
