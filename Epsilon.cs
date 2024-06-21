using System;
using System.IO;
using System.Linq;
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
* create-proj: create a new EPSLPROJ file
            ".Trim()
        );

        bool alwaysProj = false;
        parser.AddOption(() => alwaysProj = true, "Always use project mode", null,
            "p", "project-mode");
        
        bool neverProj = false;
        parser.AddOption(() => neverProj = true, "Never use project mode", null, 
            "P", "no-project-mode");

        bool disableCache = false;
        parser.AddOption(() => disableCache = true, "Disable loading modules from compilation cache",
            null, "disable-cache");

        bool linkBuiltins = true;
        parser.AddOption(() => linkBuiltins = false, "Don't link to Epsilon's builtins", null, 
            "no-builtins");

        bool linkLibraries = true;
        parser.AddOption(() => linkLibraries = false, "Don't link to Epsilon libraries", null, 
            "no-libraries");

        DelimitedInputExpectation clangOptions = parser.AddOption(
            new DelimitedInputExpectation(parser, "clang-options", "END", needsOne: true), 
            "Options for the clang compiler", "c", "clang-options"
        );

        parser.AddOption(
            new CaptureExpectation(val => clangOptions.Matched = val, "clang-option"),
            "An option for the clang compiler", "clang-option"
        );

        List<string> libraries = new List<string>();

        parser.AddOption(
            new CaptureExpectation(val => libraries.Add(val), "library-path"),
            "An additional library to load", "l", "library"
        );

        InputExpectation outputFile = new InputExpectation("output file");
        parser.AddOption(outputFile, "The path to output to", "o", "output");

        PossibilitiesExpectation mode = parser.Expect(
            new PossibilitiesExpectation("compile", "teardown", "create-proj"));
        
        InputExpectation sourceFile = new InputExpectation("source file", true);

        InputExpectation projFile = new InputExpectation("proj file location");
        
        mode.Then(() => {
            if (mode.Value() == "compile") {
                parser.Expect(sourceFile);
            } else if (mode.Value() == "teardown") {
                parser.Expect(sourceFile);
            } else if (mode.Value() == "create-proj") {
                parser.Expect(projFile);
            }
        });

        PossibilitiesExpectation outputType = parser.AddOption(
            new PossibilitiesExpectation("executable", "package", "llvm-ll", "llvm-bc"), 
            "The output file type", "t", "output-type"
        );

        PossibilitiesExpectation verbosity = parser.AddOption(
            new PossibilitiesExpectation((int)LogLevel.NONE, typeof(LogLevel)), 
            "Logging verbosity (each option implies the next)", "v", "verbosity"
        );

        parser.AddUsageOption(mode.Usage("compile"), sourceFile);
        parser.AddUsageOption(mode.Usage("teardown"), sourceFile);
        parser.AddUsageOption(mode.Usage("create-proj"), projFile);
        
        parser.Parse(args);

        Log.Verbosity = verbosity.ToEnum<LogLevel>();

        Builder builder = new Builder();

        string curDirectory = $".{Path.DirectorySeparatorChar}";
        string input = curDirectory + (sourceFile.Matched ?? "entry");

        if (mode.Value() == "compile") {
            TestResult(builder.LoadEPSLPROJ(input, neverProj, out EPSLPROJ proj));
            parser.ParseAdditionalOptions(proj.CommandOptions);
            Log.Verbosity = verbosity.ToEnum<LogLevel>();
            
            if (alwaysProj && neverProj) {
                ArgumentParser.DisplayProblem("The 'project-mode' and 'no-project-mode' options are mutually exclusive");
            }

            if (outputType.Value() == "package") {
                if (neverProj) {
                    ArgumentParser.DisplayProblem("The 'package' output type requires project mode, and thus forbids the 'no-project-mode' option");
                }
                alwaysProj = true;
                linkBuiltins = false;
                linkLibraries = false;
            }

            libraries.AddRange(proj.Libraries);

            TestResult(builder.RegisterLibraries(libraries));

            BuildSettings settings = new BuildSettings(input, proj, alwaysProj, neverProj, disableCache, linkBuiltins, linkLibraries, clangOptions.MatchedSegments);

            return DoCompilation(
                builder, settings, outputType.Value(),
                outputFile.IsPresent ? outputFile.Matched : null
            );
        } else if (mode.Value() == "teardown") {
            TestResult(builder.LoadEPSLPROJ(input, false, out EPSLPROJ proj, allowNew: false));
            Log.Verbosity = verbosity.ToEnum<LogLevel>();

            TestResult(builder.Teardown(proj));

            return 0;
        } else if (mode.Value() == "create-proj") {
            TestResult(builder.CreateEPSLPROJ(projFile.Matched));

            return 0;
        } else {
            throw new InvalidOperationException();
        }
    }

    static int DoCompilation(Builder builder, BuildSettings settings, string outputType, string providedOutput) {
        string extension;
        switch (outputType) {
            case "llvm-bc":
                extension = ".bc";
                break;
            case "llvm-ll":
                extension = ".ll";
                break;
            case "executable":
                extension = null;
                break;
            case "package":
                extension = null;
                break;
            default:
                throw new InvalidOperationException();
        }

        TestResult(builder.GetOutputLocation(settings.InputPath, extension,
            out string defaultOutput, out string outputName));
        string output = providedOutput ?? defaultOutput;

        TestResult(builder.Build(settings, out BuildInfo buildInfo));
        if (outputType == "executable") {
            TestResult(builder.ToExecutable(buildInfo));
        }

        if (outputType == "package") {
            TestResult(builder.ReadyPackageFolder(output));
            
            try {
                string bitcodeSrc = Utils.JoinPaths(
                    Utils.TempDir(), "code-linked.bc");
                File.Copy(bitcodeSrc, Utils.JoinPaths(output, outputName+".bc"), true);
            } catch (IOException) {
                ArgumentParser.DisplayProblem("Could not write specified output file");
                return 1;
            }

            IEnumerable<string> unlinkedImports = buildInfo.UnlinkedFiles
                .Select(file => file.PartialPath);

            EPSLSPEC entryEPSLSPEC = buildInfo.EntryFile.EPSLSPEC;
            EPSLSPEC newEPSLSPEC = new EPSLSPEC(
                entryEPSLSPEC.Functions, entryEPSLSPEC.Structs, Dependencies.Empty(),
                buildInfo.ClangConfig, unlinkedImports, 
                outputName+".bc", null, FileSourceType.Library,
                entryEPSLSPEC.IDPath
            );

            string epslspecDest = Utils.JoinPaths(output, outputName+".epslspec");
            TestResult(builder.SaveEPSLSPEC(epslspecDest, newEPSLSPEC));
        } else {
            string resultSource;

            switch (outputType) {
                case "llvm-bc":
                    resultSource = "code-linked.bc";
                    break;
                case "llvm-ll":
                    string bcFile = Utils.JoinPaths(
                        Utils.TempDir(), "code-linked.bc");
                    Utils.RunCommand("llvm-dis", new List<string> {
                        "--", bcFile
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
                string absoluteResultSource = Utils.JoinPaths(
                    Utils.TempDir(), resultSource);
                File.Copy(absoluteResultSource, output, true);
            } catch (IOException) {
                ArgumentParser.DisplayProblem("Could not write specified output file");
                return 1;
            }
        }

        return 0;
    }

    static void TestResult(ResultStatus result) {
        if (result != ResultStatus.GOOD) {
            Environment.Exit(1);
        }
    }
}
