using System;
using System.IO;
using System.Collections.Generic;

public class Epsilon {
    public static int Main(string[] args) {
        Utils.LoadCommands(new List<string> {
            "bash", "python", "opt", "clang", "python", "llvm-link"
        });
        
        ArgumentParser parser = new ArgumentParser();
        
        parser.AddBranch("compile");
        parser.AddBranch("*input file");
        parser.AddLeaf("*output file");
        
        ParserResults parseResults = parser.Parse(args);
        List<string> mode = parseResults.GetMode();
        List<string> values = parseResults.GetValues();
        
        if (mode[0] == "compile") {
            Builder builder = new Builder();
            
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
