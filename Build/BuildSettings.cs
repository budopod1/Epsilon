using System;
using System.Linq;
using System.Collections.Generic;

public class BuildSettings(string inputPath, string providedOutput, EPSLPROJ proj, EPSLCACHE cache, CacheMode cacheMode, OptimizationLevel optLevel, OutputType outputType, bool linkBuiltins, bool linkLibraries) {
    public string InputPath = inputPath;
    public string ProvidedOutput = providedOutput;
    public EPSLPROJ Proj = proj;
    public EPSLCACHE Cache = cache;
    public CacheMode CacheMode = cacheMode;
    public OptimizationLevel OptLevel = optLevel;
    public OutputType Output_Type = outputType;
    public bool LinkBuiltins = linkBuiltins;
    public bool LinkLibraries = linkLibraries;
}
