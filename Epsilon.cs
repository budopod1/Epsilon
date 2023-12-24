using System;
using System.IO;
using System.Collections.Generic;

public class Epsilon {
    public static int Main(string[] args) {
        ArgumentParser parser = new ArgumentParser();

        parser.AddOption("p", "Print the AST");
        parser.AddOption("print-ast", "Print the AST");
        parser.AddOption("s", "Print the compilation steps");
        parser.AddOption("print-steps", "Print the compilation steps");
        parser.AddOption("t", "Show step timings");
        parser.AddOption("timings", "Show step timings");
        parser.AddOption("c", "Do not catch errors");
        parser.AddOption("do-not-catch-errs", "Do not catch errors");
        parser.AddOption("w", "Do not write to the output file");
        parser.AddOption("do-not-write-output", "Do not write to the output file");
        
        parser.AddBranch("compile");
        parser.AddBranch("*input file");
        parser.AddLeaf("*output file");
        
        ParserResults parseResults = parser.Parse(args);
        List<string> mode = parseResults.GetMode();
        List<string> values = parseResults.GetValues();
        
        if (mode[0] == "compile") {
            Compiler compiler = new Compiler();
            compiler.PRINT_AST = parseResults.HasOption("p", "print-ast");
            compiler.PRINT_STEPS = parseResults.HasOption("s", "print-steps");
            compiler.SHOW_TIMINGS = parseResults.HasOption("t", "timings");
            compiler.CATCH_ERRS = !parseResults.HasOption("c", "do-not-catch-errs");
            bool doNotWriteOutput = parseResults.HasOption("w", "do-not-write-output");
            
            string input = values[0];
            string output = values[1];
            string content = null;
            try {
                using (StreamReader file = new StreamReader(input)) {
                    content = file.ReadToEnd();
                }
            } catch (IOException) {
                parser.DisplayProblem("Could not read specified input file");
                return 1;
            }
            CompilationResultStatus resultStatus = compiler.Compile(input, content);
            if (resultStatus != CompilationResultStatus.GOOD) return 1;
            if (doNotWriteOutput) return 0;
            int status = compiler.CompileIR();
            if (status > 0) return status;
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
