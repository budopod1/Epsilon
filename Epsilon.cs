using System;
using System.IO;
using System.Collections.Generic;

public class App {
    public static void Main(string[] args) {
        ArgumentParser parser = new ArgumentParser();
        
        parser.AddBranch("compile");
        parser.AddBranch("*input file");
        parser.AddLeaf("*output file");
        
        ParserResults parseResults = parser.Parse(args);
        List<string> mode = parseResults.GetMode();
        List<string> values = parseResults.GetValues();
        
        if (mode[0] == "compile") {
            Compiler compiler = new Compiler();
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
