using System;
using System.Linq;
using System.Collections.Generic;

public class BuildSettings {
    public string InputPath;
    public string ProvidedOutput;
    public EPSLPROJ Proj;
    public EPSLCACHE Cache;
    public CacheMode CacheMode;
    public OptimizationLevel OptLevel;
    public OutputType Output_Type;
    public bool LinkBuiltins;
    public bool LinkLibraries;
    public IEnumerable<string> ExtraClangOptions;

    public BuildSettings(string inputPath, string providedOutput, EPSLPROJ proj, EPSLCACHE cache, CacheMode cacheMode, OptimizationLevel optLevel, OutputType outputType, bool linkBuiltins, bool linkLibraries, IEnumerable<string> extraClangOptions) {
        InputPath = inputPath;
        ProvidedOutput = providedOutput;
        Proj = proj;
        Cache = cache;
        CacheMode = cacheMode;
        OptLevel = optLevel;
        Output_Type = outputType;
        LinkBuiltins = linkBuiltins;
        LinkLibraries = linkLibraries;
        ExtraClangOptions = extraClangOptions;
    }
}
