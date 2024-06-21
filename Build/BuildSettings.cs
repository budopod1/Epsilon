using System;
using System.Linq;
using System.Collections.Generic;

public class BuildSettings {
    public string InputPath;
    public EPSLPROJ Proj;
    public bool AlwaysProject;
    public bool NeverProject;
    public bool DisableCache;
    public bool LinkBuiltins;
    public bool LinkLibraries;
    public IEnumerable<string> ExtraClangOptions;

    public BuildSettings(string inputPath, EPSLPROJ proj, bool alwaysProject, bool neverProject, bool disableCache, bool linkBuiltins, bool linkLibraries, IEnumerable<string> extraClangOptions) {
        InputPath = inputPath;
        Proj = proj;
        AlwaysProject = alwaysProject;
        NeverProject = neverProject;
        DisableCache = disableCache;
        LinkBuiltins = linkBuiltins;
        LinkLibraries = linkLibraries;
        ExtraClangOptions = extraClangOptions;
    }
}
