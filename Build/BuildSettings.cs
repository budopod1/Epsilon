namespace Epsilon;

public class BuildSettings(string inputPath, string providedOutput, EPSLCACHE cache,
        CacheMode cacheMode, OptimizationLevel optLevel, OutputType outputType,
        bool generateErrorFrames, bool linkBuiltins, bool linkLibraries,
        bool linkBuiltinModules) {
    public string InputPath = inputPath;
    public string ProvidedOutput = providedOutput;
    public EPSLCACHE Cache = cache;
    public CacheMode CacheMode = cacheMode;
    public OptimizationLevel OptLevel = optLevel;
    public bool GenerateErrorFrames = generateErrorFrames;
    public OutputType Output_Type = outputType;
    public bool LinkBuiltins = linkBuiltins;
    public bool LinkLibraries = linkLibraries;
    public bool LinkBuiltinModules = linkBuiltinModules;

    public string GetIDPath(string path) {
        path = Utils.Stem(path);
        DirectoryInfo inputDir = new(Utils.GetDirectoryName(InputPath));
        string relPath = Path.GetRelativePath(inputDir.Parent.FullName, path);
        if (relPath.StartsWith("..")) {
            return path;
        } else {
            return Path.DirectorySeparatorChar + relPath;
        }
    }
}
