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
* compile: compile specified file
* teardown: remove the cached compilation files
* command-line-options: specifies the command line options to be used with a project
            ".Trim()
        );

        bool alwaysProj = false;
        bool neverProj = false;

        parser.AddOption(() => {alwaysProj = true;}, "Always use project mode", null,
            "p", "project-mode");
        parser.AddOption(() => {neverProj = true;}, "Never use project mode", null, 
            "P", "no-project-mode");

        InputExpectation outputFile = new InputExpectation("output file");
        parser.AddOption(outputFile, "The path to output to", "o", "output");

        PossibilitiesExpectation mode = parser.Expect(
            new PossibilitiesExpectation("compile", "teardown", "command-line-options"));
        
        InputExpectation sourceFile = new InputExpectation("source file", true);

        MultiInputExpectation commandOptions = new MultiInputExpectation(parser, "command-options");
        
        mode.Then(() => {
            if (mode.Value() == "compile") {
                parser.Expect(sourceFile);
            } else if (mode.Value() == "teardown") {
                parser.Expect(sourceFile);
            } else if (mode.Value() == "command-line-options") {
                parser.Expect(sourceFile, commandOptions);
            }
        });

        PossibilitiesExpectation outputType = parser.AddOption(
            new PossibilitiesExpectation("executable", "llvm-ll", "llvm-bc"), 
            "The output file type", "t", "output-type"
        );

        PossibilitiesExpectation verbosity = parser.AddOption(
            new PossibilitiesExpectation((int)LogLevel.NONE, typeof(LogLevel)), 
            "Logging verbosity (each option implies the next)", "v", "verbosity"
        );

        parser.AddUsageOption(mode.Usage("compile"), sourceFile);
        parser.AddUsageOption(mode.Usage("teardown"), sourceFile);
        parser.AddUsageOption(mode.Usage("command-line-options"), sourceFile, 
            new CmdUsagePart("--"), commandOptions);
        
        parser.Parse(args);

        Log.Verbosity = verbosity.ToEnum<LogLevel>();

        Builder builder = new Builder();

        string curDirectory = $".{Path.DirectorySeparatorChar}";
        string input = curDirectory + (sourceFile.Matched ?? "entry");

        if (mode.Value() == "compile") {
            builder.NEVER_PROJECT = neverProj;
            TestResult(builder.LoadEPSLPROJ(input, out EPSLPROJ proj));
            parser.ParseAdditionalOptions(proj.CommandOptions);
            Log.Verbosity = verbosity.ToEnum<LogLevel>();
            
            if (alwaysProj && neverProj) {
                ArgumentParser.DisplayProblem("The 'project-mode' and 'no-project-mode' options are mutually exclusive");
            }

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
                TestResult(builder.GetOutputLocation(input, extension, out output));
            }
    
            builder.ALWAYS_PROJECT = alwaysProj;
            builder.NEVER_PROJECT = neverProj;
            
            TestResult(builder.Build(input, proj));
            if (outputType.Value() == "executable") {
                TestResult(builder.ToExecutable());
            }
    
            string resultSource;
            
            switch (outputType.Value()) {
                case "llvm-bc":
                    resultSource = "code-linked.bc";
                    break;
                case "llvm-ll":
                    Utils.RunCommand("llvm-dis", new List<string> {
                        "--", Utils.JoinPaths(Utils.ProjectAbsolutePath(), "code-linked.bc")
                    });
                    resultSource = "code-linked.ll";
                    break;
                case "executable":
                    resultSource = "code";
                    break;
                default:
                    throw new InvalidOperationException();
            }
            
            try {
                string executable = Utils.JoinPaths(Utils.ProjectAbsolutePath(), resultSource);
                File.Copy(executable, output, true);
            } catch (IOException) {
                ArgumentParser.DisplayProblem("Could not write specified output file");
                return 1;
            }
            
            return 0;
        } else if (mode.Value() == "teardown") {
            TestResult(builder.LoadEPSLPROJ(input, out EPSLPROJ proj));
            Log.Verbosity = verbosity.ToEnum<LogLevel>();

            if (alwaysProj || neverProj) {
                ArgumentParser.DisplayProblem("The 'project-mode' and 'no-project-mode' options require 'compile' mode");
            }

            if (outputType.IsPresent) {
                ArgumentParser.DisplayProblem("The 'output-type' option requires 'compile' mode");
            }

            if (outputFile.IsPresent) {
                ArgumentParser.DisplayProblem("The 'output' option requires 'compile' mode");
            }

            TestResult(builder.Teardown(proj));

            return 0;
        } else if (mode.Value() == "command-line-options") {
            TestResult(builder.LoadEPSLPROJ(input, out EPSLPROJ proj));

            if (alwaysProj || neverProj) {
                ArgumentParser.DisplayProblem("The 'project-mode' and 'no-project-mode' options require 'compile' mode");
            }

            if (outputType.IsPresent) {
                ArgumentParser.DisplayProblem("The 'output-type' option requires 'compile' mode");
            }

            if (outputFile.IsPresent) {
                ArgumentParser.DisplayProblem("The 'output' option requires 'compile' mode");
            }

            TestResult(builder.SetEPSLPROJOptionAndSave(proj, commandOptions.MatchedSegments));

            return 0;
        } else {
            throw new InvalidOperationException();
        }
    }

    static void TestResult(CompilationResult result) {
        if (result.GetStatus() != CompilationResultStatus.GOOD) {
            if (result.HasMessage()) ArgumentParser.DisplayProblem(result.GetMessage());
            Environment.Exit(1);
        }
    }
}
