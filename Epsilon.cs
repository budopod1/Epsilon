using System;
using System.IO;
using System.Collections.Generic;

public class App {
    public static void Main(string[] args) {
        ArgumentParser parser = new ArgumentParser();

        parser.AddOption("p", "Print the AST");
        parser.AddOption("print-ast", "Print the AST");
        parser.AddOption("s", "Print the AST");
        parser.AddOption("print-steps", "Print the compilation steps");
        parser.AddOption("t", "Show step timings");
        parser.AddOption("timings", "Show step timings");
        
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
            
            string input = values[0];
            string output = values[1];
            string content = null;
            try {
                using (StreamReader file = new StreamReader(input)) {
                    content = file.ReadToEnd();
                }
            } catch (IOException) {
                parser.DisplayProblem("Could not read specified input file");
            }
            if (content != null)
                compiler.Compile(input, content);
        }
    }
}
