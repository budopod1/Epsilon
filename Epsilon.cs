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
        
        InputExpectation projFile = new InputExpectation("project file");
        
        mode.Then(() => {
            if (mode.Value() == "compile") {
                parser.Expect(inputFile, outputFile);
            } else if (mode.Value() == "teardown") {
                parser.Expect(projFile);
            }
        });

        PossibilitiesExpectation outputType = parser.AddOption(
            new PossibilitiesExpectation("executable", "llvm-ll", "llvm-bc"), 
            "The output file type", "t", "output-type"
        );

        parser.AddUsageOption(mode, inputFile, outputFile);
        parser.AddUsageOption(mode, projFile);
        
        parser.Parse(args);

        Builder builder = new Builder();

        if (mode.Value() == "compile") {
            if (alwaysProj && neverProj) {
                parser.DisplayProblem("The 'project-mode' and 'no-project-mode' options are mutually exclusive");
            }
    
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
        } else if (mode.Value() == "teardown") {
            if (alwaysProj || neverProj) {
                parser.DisplayProblem("The 'project-mode' and 'no-project-mode' options require 'compile' mode");
            }

            if (outputType.IsPresent) {
                parser.DisplayProblem("The 'output-type' option requires 'compile' mode");
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
