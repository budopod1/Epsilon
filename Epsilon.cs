using CsCommandLine;

namespace Epsilon;
public class Epsilon {
    public static void Main(string[] args) {
        ArgumentParser parser = new(
            "epslc",
            @"
A compiler for the Epsilon programming language

Modes:
* compile: compile specified file
* teardown: remove the cached compilation files
* create-proj: create a new EPSLPROJ file
            ".Trim()
        );

        PossibilitiesExpectation cacheModeInput = parser.AddOption(
            new PossibilitiesExpectation("auto", "dont-use", "dont-load", "auto", "always"),
            "Caching mode", "H", "cache-mode"
        );

        PossibilitiesExpectation optimizationInput = parser.AddOption(
            new PossibilitiesExpectation("min", "0", "min", "1", "normal", "2", "max"),
            "Optimization level", "O", "opt"
        );

        bool linkBuiltins = true;
        parser.AddOption(() => linkBuiltins = false, "Don't link to Epsilon's builtins", null,
            "no-builtins");

        bool linkLibraries = true;
        parser.AddOption(() => linkLibraries = false, "Don't link to Epsilon libraries", null,
            "no-libraries");

        DelimitedInputExpectation clangParseOptions = parser.AddOption(
            new DelimitedInputExpectation(parser, "clang-parse-options", "END", needsOne: true),
            "Options for clang to use while parsing C/C++", "C", "clang-parse-options"
        );

        parser.AddOption(
            new CaptureExpectation(val => clangParseOptions.Matched = val, "clang-option"),
            "An option for the clang compiler", "clang-parse-option"
        );

        DelimitedInputExpectation linkingOptions = parser.AddOption(
            new DelimitedInputExpectation(parser, "linking-options", "END", needsOne: true),
            "Options for clang to use while parsing C/C++", "L", "linking-options"
        );

        parser.AddOption(
            new CaptureExpectation(val => clangParseOptions.Matched = val, "linking-option"),
            "An option for the clang compiler", "-L", "linking-option"
        );

        List<string> libraries = [];

        parser.AddOption(
            new CaptureExpectation(val => libraries.Add(val), "library-path"),
            "An additional library to load", "l", "library"
        );

        InputExpectation outputFile = new("output file");
        parser.AddOption(outputFile, "The path to output to", "o", "output");

        PossibilitiesExpectation mode = parser.Expect(
            new PossibilitiesExpectation("compile", "compile", "teardown", "create-proj"));

        InputExpectation sourceFile = new("source file", optional: true);

        InputExpectation projFile = new("proj file location");

        mode.Then(() => {
            if (mode.Value() == "compile") {
                parser.Expect(sourceFile);
            } else if (mode.Value() == "teardown") {
                parser.Expect(sourceFile);
            } else if (mode.Value() == "create-proj") {
                parser.Expect(projFile);
            }
        });

        PossibilitiesExpectation outputTypeInput = parser.AddOption(new PossibilitiesExpectation(
            "executable", "executable", "llvm-ll", "llvm-bc", "package-both", "package-obj",
            "object", "shared-object"
        ), "The output file type", "t", "output-type");

        PossibilitiesExpectation verbosity = parser.AddOption(
            new PossibilitiesExpectation((int)LogLevel.NONE, typeof(LogLevel)),
            "Logging verbosity (each option implies the next)", "V", "verbosity"
        );

        bool verbose = false;
        parser.AddOption(() => verbose = true, "Use maximally verbose output", null,
            "v", "verbose");

        parser.AddUsageOption(mode.Usage("compile"), sourceFile);
        parser.AddUsageOption(mode.Usage("teardown"), sourceFile);
        parser.AddUsageOption(mode.Usage("create-proj"), projFile);

        parser.Parse(args);

        Log.Verbosity = verbosity.ToEnum<LogLevel>();
        if (verbose) Log.Verbosity = LogLevel.TEMP;

        Builder builder = new();

        TestResult(builder.WipeTempDir());

        TestResult(builder.ComputeInputPath(
            sourceFile.Matched ?? projFile.Matched, out string input));

        if (mode.Value() == "compile") {
            TestResult(builder.LoadEPSLPROJ(input, out EPSLPROJ proj));
            TestResult(builder.ParseAdditionalOptions(parser, proj));

            Subconfigs.AddClangParseConfigs(clangParseOptions.MatchedSegments);
            Subconfigs.AddLinkingConfigs(linkingOptions.MatchedSegments);

            CacheMode cacheMode = EnumHelpers.ParseCacheMode(cacheModeInput.Value());
            OptimizationLevel optimizationLevel = EnumHelpers.ParseOptimizationLevel(optimizationInput.Value());
            OutputType outputType = EnumHelpers.ParseOutputType(outputTypeInput.Value());

            if (outputType.MustntLinkBuiltins()) linkBuiltins = false;
            if (outputType.MustntLinkLibraries()) linkLibraries = false;

            libraries.AddRange(proj.Libraries);

            TestResult(builder.RegisterLibraries(input, libraries));
            TestResult(builder.LoadEPSLCACHE(input, cacheMode, out EPSLCACHE cache));

            if (cacheMode > CacheMode.DONTLOAD && EPSLCACHE.MustDiscardCache(cache.LastOutputType, outputType)) {
                Log.Info($"Cached data generated with the previous output type, {cache.LastOutputType}, cannot be used with the current output type, {outputType}. As such, all cached data is being disregarded.");
                cacheMode = CacheMode.DONTLOAD;
            }

            string providedOutput = outputFile.IsPresent ? outputFile.Matched : null;

            BuildSettings settings = new(
                input, providedOutput, proj, cache, cacheMode, optimizationLevel, outputType,
                linkBuiltins, linkLibraries
            );

            TestResult(builder.Build(settings));
        } else if (mode.Value() == "teardown") {
            TestResult(builder.LoadEPSLCACHE(input, CacheMode.AUTO, out EPSLCACHE cache));

            TestResult(builder.Teardown(cache));
        } else if (mode.Value() == "create-proj") {
            TestResult(builder.CreateEPSLPROJ(input));
        } else {
            throw new InvalidOperationException();
        }
    }

    static void TestResult(ResultStatus result) {
        if (result != ResultStatus.GOOD) {
            Environment.Exit(1);
        }
    }
}
