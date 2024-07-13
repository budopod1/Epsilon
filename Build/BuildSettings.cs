using System;
using System.Linq;
using System.Collections.Generic;

public class BuildSettings {
    public string InputPath;
    public EPSLPROJ Proj;
    public EPSLCACHE Cache;
    public CacheMode CacheMode;
    public OptimizationLevel OptLevel;
    public bool LinkBuiltins;
    public bool LinkLibraries;
    public IEnumerable<string> ExtraClangOptions;

    public BuildSettings(string inputPath, EPSLPROJ proj, EPSLCACHE cache, CacheMode cacheMode, OptimizationLevel optLevel, bool linkBuiltins, bool linkLibraries, IEnumerable<string> extraClangOptions) {
        InputPath = inputPath;
        Proj = proj;
        Cache = cache;
        CacheMode = cacheMode;
        OptLevel = optLevel;
        LinkBuiltins = linkBuiltins;
        LinkLibraries = linkLibraries;
        ExtraClangOptions = extraClangOptions;
    }
}
