using System;
using System.IO;
using System.Collections.Generic;

public class Epsilon {
    public static int Main(string[] args) {
        ArgumentParser parser = new ArgumentParser(
            "mono Epsilon.exe",
            @"
A compiler for the Epsilon programming language

Modes:
* compile: compile specified files
* teardown: remove the cached compiling files
            ".Trim()
        );

        bool alwaysProj = false;
        bool neverProj = false;

        parser.AddOption(() => {alwaysProj = true;}, "Always use project mode", 
            "p", "project-mode");
        parser.AddOption(() => {neverProj = true;}, "Never use project mode", 
            "P", "no-project-mode");

        PossibilitiesExpectation mode = parser.Expect(
            new PossibilitiesExpectation("compile", "teardown"));
        
        InputExpectation inputFile = new InputExpectation("input file");
        InputExpectation outputFile = new InputExpectation("output file");
        parser.AddOption(outputFile, "The path to output to", "o", "output");
        
        InputExpectation projFile = new InputExpectation("project file");
        
        mode.Then(() => {
            if (mode.Value() == "compile") {
                parser.Expect(inputFile);
            } else if (mode.Value() == "teardown") {
                parser.Expect(projFile);
            }
        });

        PossibilitiesExpectation outputType = parser.AddOption(
            new PossibilitiesExpectation("executable", "llvm-ll", "llvm-bc"), 
            "The output file type", "t", "output-type"
        );

        PossibilitiesExpectation verbosity = parser.AddOption(
            new PossibilitiesExpectation((int)LogLevel.NONE, typeof(LogLevel)), 
            "Logging verbosity (each option implies the previous)", "v", "verbosity"
        );

        parser.AddUsageOption(mode.Usage("compile"), inputFile);
        parser.AddUsageOption(mode.Usage("teardown"), projFile);
        
        parser.Parse(args);

        Log.Verbosity = verbosity.ToEnum<LogLevel>();

        Builder builder = new Builder();

        if (mode.Value() == "compile") {
            if (alwaysProj && neverProj) {
                parser.DisplayProblem("The 'project-mode' and 'no-project-mode' options are mutually exclusive");
            }

            string input = Utils.GetFullPath(inputFile.Matched);

            string extension;
            
            switch (outputType.Value()) {
                case "llvm-bc":
                    extension = ".bc";
                    break;
                case "llvm-ll":
                    extension = ".ll";
                    break;
                case "executable":
                    extension = null;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            string output = null;
            if (outputFile.IsPresent) {
                output = outputFile.Matched;
            } else {
                string outputDir = Utils.GetDirectoryName(input);
                string outputName = Utils.SetExtension(
                    Utils.GetFileName(input), extension);
                if (outputName == "entry") outputName = "result";
                output = Utils.JoinPaths(outputDir, outputName);
            }
    
            builder.ALWAYS_PROJECT = alwaysProj;
            builder.NEVER_PROJECT = neverProj;
            
            CompilationResult result = builder.Build(input);
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
                File.Copy(executable, output, true);
            } catch (IOException) {
                parser.DisplayProblem("Could not write specified output file");
                return 1;
            }
            
            return 0;
        } else if (mode.Value() == "teardown") {
            if (alwaysProj || neverProj) {
                parser.DisplayProblem("The 'project-mode' and 'no-project-mode' options require 'compile' mode");
            }

            if (outputType.IsPresent) {
                parser.DisplayProblem("The 'output-type' option requires 'compile' mode");
            }

            if (outputFile.IsPresent) {
                parser.DisplayProblem("The 'output' option requires 'compile' mode");
            }

            CompilationResult result = builder.Teardown(projFile.Matched);
            
            if (result.GetStatus() != CompilationResultStatus.GOOD) {
                if (result.HasMessage()) parser.DisplayProblem(result.GetMessage());
                return 1;
            }

            return 0;
        } else {
            throw new InvalidOperationException();
        }
    }
}
