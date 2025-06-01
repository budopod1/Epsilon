using CsCommandConfig;

namespace Epsilon;
public class Epsilon {
    public static void Main(string[] args) {
        ProgramConfiguration config = new();

        config.AddOption(new StringsOption("branch"));
        config.AddOption(new StringOption("file"));
        config.AddOption(new StringOption("package-name"));
        config.AddOption(new StringOption("url"));
        config.AddOption(new EnumOption("verbosity", typeof(LogLevel), "NONE"));

        config.AddOption(new EnumOption("cache-mode", ["dont-use", "dont-load", "auto", "always"], "AUTO"));
        config.AddOption(new EnumOption("optimization-level", ["0", "min", "1", "normal", "2", "max"], "min"));
        config.AddOption(new BoolOption("generate-error-frames", true));
        config.AddOption(new BoolOption("link-builtins", true));
        config.AddOption(new BoolOption("link-libraries", true));
        config.AddOption(new BoolOption("link-builtin-modules", true));
        config.AddOption(new StringsOption("clang-parse-options"));
        config.AddOption(new StringsOption("linking-options"));
        config.AddOption(new StringsOption("object-gen-options"));
        config.AddOption(new StringsOption("packages"));
        config.AddOption(new StringOption("output-location"));
        config.AddOption(new EnumOption("output-type", ["executable", "llvm-ll", "llvm-bc",
            "package-both", "package-obj", "object", "shared-object"], "executable"));

        config.AddOption(new BoolOption("all-packages", false));

        Builder.EPSLPROJShape = JSONConfigParser.GetShape(config);

        ArgumentParser parser = new(
            config, "epslc",
            "A compiler for the Epsilon programming language",
            priority: 1
        );

        parser.Flag(["verbose", "v"], "verbosity",
            new ConstArgumentValue<string>("TEMP"), "Provide verbose output");
        parser.Flag(["verbosity", "V"], "verbosity",
            new StringArgumentValue(), "Provide output of the specified verbosity");

        parser.Tree("compile");
        parser.SetBranchHelp("Compile an Epsilon file or project");

        parser.Positional("file", new StringArgumentValue(),
            "The file or project to compile", isOptional: true);

        parser.Flag(["cache-mode", "H"], "cache-mode", new StringArgumentValue(),
            "The mode to use when loading and saving parital compilation files.");
        parser.Flag(["opt", "O"], "optimization-level", new StringArgumentValue(),
            "The optimization level");

        parser.Flag(["no-generate-error-frames", "G"], "generate-error-frames",
            new ConstArgumentValue<bool>(false), "Don't generate error frames");

        parser.Flag(["no-link-builtins"], "link-builtins", new ConstArgumentValue<bool>(false),
            "Don't link the result to Epsilon's builtins");
        parser.Flag(["no-link-libraries"], "link-libraries", new ConstArgumentValue<bool>(false),
            "Don't link the result to sources from libraries");
        parser.Flag(["no-link-builtin-modules"], "link-builtin-modules", new ConstArgumentValue<bool>(false),
            "Don't link the result to Epsilon's builtin module's sources");

        parser.Flag(["clang-parse-options", "C"], "clang-parse-options", new MultiStringArgumentValue(terminator: "END"),
            "Additional options to send to clang when parsing C or C++ code");
        parser.Flag(["clang-parse-option", "c"], "clang-parse-options", new StringArgumentValue(),
            "An option to send to clang when parsing C or C++ code");

        parser.Flag(["linking-options", "Z"], "linking-options", new MultiStringArgumentValue(terminator: "END"),
            "Additional options to send to clang or ld when linking");
        parser.Flag(["linking-option", "z"], "linking-options", new StringArgumentValue(),
            "An option to send to clang or ld when linking");

        parser.Flag(["object-gen-options", "G"], "object-gen-options", new MultiStringArgumentValue(terminator: "END"),
            "Additional options to send to llc or clang when generating object files");
        parser.Flag(["object-gen-option", "g"], "object-gen-options", new StringArgumentValue(),
            "An option to send to send to llc or clang when generating object files");

        parser.Flag(["package", "p"], "packages", new StringArgumentValue(),
            "A package to include during compilation");

        parser.Flag(["output", "o"], "output-location", new StringArgumentValue(),
            "The location to place the output in");
        parser.Flag(["output-type", "t"], "output-type", new StringArgumentValue(),
            "The type of output to produce");

        parser.Tree("clean");
        parser.SetBranchHelp("Clean up all of a project's temporary files");
        parser.Positional("file", new StringArgumentValue(),
            "The .epslproj file");

        parser.Tree("init");
        parser.SetBranchHelp("Create a new Epsilon project");
        parser.Positional("file", new StringArgumentValue(),
            "The location of the new project file", isOptional: true);

        parser.Tree("package");
        parser.SetBranchHelp("Manage Epsilon packages");

        parser.Tree("package", "add");
        parser.SetBranchHelp("Add a package to a project");
        parser.Positional("package-name", new StringArgumentValue(),
            "The name of the package to add");
        parser.Flag(["project", "P"], "file", new StringArgumentValue(),
            "The project to add the package to");

        parser.Tree("package", "remove");
        parser.SetBranchHelp("Remove a package from a project");
        parser.Positional("package-name", new StringArgumentValue(),
            "The name of the package to remove");
        parser.Flag(["project", "p"], "file", new StringArgumentValue(),
            "The project to remove the package from");

        parser.Tree("package", "list");
        parser.SetBranchHelp("List a project's packages");
        parser.Positional("file", new StringArgumentValue(),
            "The project to list the packages of", isOptional: true);
        parser.Flag(["all", "a"], "all-packages", new ConstArgumentValue<bool>(true),
            "List all packages");

        parser.Tree("package", "register");
        parser.SetBranchHelp("Register the current project as a package");
        parser.Flag(["project", "p"], "file", new StringArgumentValue(),
            "The project to register as a package");

        parser.Tree("package", "install");
        parser.SetBranchHelp("Install a package from the specified URL");
        parser.Positional("url", new StringArgumentValue(),
            "The URL to install the package from");
        parser.Flag(["project", "p"], "file", new StringArgumentValue(),
            "The project to add the package to");

        parser.Tree("package", "uninstall");
        parser.SetBranchHelp("Uninstall a package");
        parser.Positional("package-name", new StringArgumentValue(),
            "The name of the package to uninstall");
        parser.Flag(["project", "p"], "file", new StringArgumentValue(),
            "The project to remove the package from");

        parser.Parse(args);

        Log.Verbosity = EnumOption.EnumProp<LogLevel>(config, "verbosity");

        Builder builder = new();

        TestResult(builder.WipeTempDir());

        TestResult(builder.ComputeInputPath(config["file"], out string inputPath));

        if (Utils.ItemsEqual(config["branch"], "compile")) {
            JSONConfigParser EPSLPROJParser = new(config, json => {
                if (json.HasKey("file")) {
                    inputPath = Utils.JoinPaths(
                        Utils.GetDirectoryName(inputPath),
                        json["file"].GetString()
                    );
                }
            }, priority: 0);
            TestResult(builder.LoadEPSLPROJ(inputPath, EPSLPROJParser));

            Subconfigs.AddClangParseConfigs(config["clang-parse-options"]);
            Subconfigs.AddLinkingConfigs(config["linking-options"]);
            Subconfigs.AddObjectGenConfigs(config["object-gen-options"]);

            CacheMode cacheMode = EnumHelpers.ParseCacheMode(config["cache-mode"]);
            OptimizationLevel optimizationLevel = EnumHelpers.ParseOptimizationLevel(
                config["optimization-level"]);
            bool generateErrorFrames = config["generate-error-frames"]
                && optimizationLevel == OptimizationLevel.MIN;
            OutputType outputType = EnumHelpers.ParseOutputType(config["output-type"]);

            bool linkBuiltins = config["link-builtins"] && !outputType.MustntLinkBuiltins();
            bool linkLibraries = config["link-libraries"] && !outputType.MustntLinkLibraries();
            bool linkBuiltinModules = config["link-builtin-modules"] && !outputType.MustntLinkBuiltinModules();

            TestResult(builder.RegisterPackages(config["packages"]));
            TestResult(builder.LoadEPSLCACHE(inputPath, cacheMode, out EPSLCACHE cache));

            if (cacheMode > CacheMode.DONTLOAD && EPSLCACHE.MustDiscardCache(cache.LastOutputType, outputType)) {
                Log.Info($"Cached data generated with the previous output type, {cache.LastOutputType}, cannot be used with the current output type, {outputType}. As such, all cached data is being disregarded.");
                cacheMode = CacheMode.DONTLOAD;
            }

            BuildSettings settings = new(
                inputPath, config["output-location"], cache, cacheMode, optimizationLevel,
                outputType, generateErrorFrames, linkBuiltins, linkLibraries, linkBuiltinModules
            );

            TestResult(builder.Build(settings));
        } else if (Utils.ItemsEqual(config["branch"], "clean")) {
            TestResult(builder.LoadEPSLCACHE(inputPath, CacheMode.AUTO, out EPSLCACHE cache));

            TestResult(builder.Teardown(cache));
        } else if (Utils.ItemsEqual(config["branch"], "init")) {
            TestResult(builder.CreateEPSLPROJ(inputPath));
        } else if (Utils.ItemsEqual(config["branch"], "package", "add")) {
            TestResult(builder.AddPackageToEPSLPROJ(config["package-name"], inputPath));
        } else if (Utils.ItemsEqual(config["branch"], "package", "remove")) {
            TestResult(builder.RemovePackageFromEPSLPROJ(config["package-name"], inputPath));
        } else if (Utils.ItemsEqual(config["branch"], "package", "list")) {
            TestResult(builder.ListPackages(inputPath, config["all-packages"]));
        } else if (Utils.ItemsEqual(config["branch"], "package", "register")) {
            TestResult(builder.RegisterProjectAsPackage(inputPath));
        } else if (Utils.ItemsEqual(config["branch"], "package", "install")) {
            TestResult(builder.InstallPackage(config["url"], inputPath));
        } else if (Utils.ItemsEqual(config["branch"], "package", "uninstall")) {
            TestResult(builder.UninstallPackage(config["package-name"], inputPath));
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
