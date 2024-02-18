using System;
using System.IO;
using System.Collections.Generic;

public class Epsilon {
    public static int Main(string[] args) {
        Utils.LoadCommands(new List<string> {
            "bash", "python", "opt", "clang", "python", "llvm-link"
        });
        
        ArgumentParser parser = new ArgumentParser();

        parser.AddOption("p", "Print the AST");
        parser.AddOption("print-ast", "Print the AST");
        parser.AddOption("s", "Print the compilation steps");
        parser.AddOption("print-steps", "Print the compilation steps");
        parser.AddOption("t", "Show step timings");
        parser.AddOption("timings", "Show step timings");
        parser.AddOption("c", "Do not catch errors");
        parser.AddOption("do-not-catch-errs", "Do not catch errors");
        
        parser.AddBranch("compile");
        parser.AddBranch("*input file");
        parser.AddLeaf("*output file");
        
        ParserResults parseResults = parser.Parse(args);
        List<string> mode = parseResults.GetMode();
        List<string> values = parseResults.GetValues();
        
        if (mode[0] == "compile") {
            Builder builder = new Builder();
            /*
            builder.PRINT_AST = parseResults.HasOption("p", "print-ast");
            builder.PRINT_STEPS = parseResults.HasOption("s", "print-steps");
            builder.SHOW_TIMINGS = parseResults.HasOption("t", "timings");
            builder.CATCH_ERRS = !parseResults.HasOption("c", "do-not-catch-errs");
            */
            
            string input = values[0];
            string output = values[1];
            
            CompilationResult result = builder.Build(input);
            if (result.GetStatus() == CompilationResultStatus.GOOD) {
                result = builder.ToExecutable();
            }
            if (result.GetStatus() != CompilationResultStatus.GOOD) {
                if (result.HasMessage()) parser.DisplayProblem(result.GetMessage());
                return 1;
            }
            try {
                File.Copy(Utils.ProjectAbsolutePath()+"/code", output, true);
            } catch (IOException) {
                parser.DisplayProblem("Could not write specified output file");
                return 1;
            }
            return 0;
        }
        return 0;
    }
}
