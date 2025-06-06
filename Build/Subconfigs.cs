using System.Runtime.InteropServices;

namespace Epsilon;
public static class Subconfigs {
    static readonly List<ISubconfig> ClangParseConfigs = [];
    static readonly List<ISubconfig> LinkingConfigs = [];
    static readonly List<ISubconfig> ObjectGenConfigs = [];

    static readonly Dictionary<ISubconfig, IEnumerable<string>> cache = [];

    static IEnumerable<string> ExpandSubconfigs(IEnumerable<ISubconfig> subconfigs) {
        return subconfigs.SelectMany(subconfig => {
            if (cache.TryGetValue(subconfig, out IEnumerable<string> cached)) {
                return cached;
            } else {
                return cache[subconfig] = subconfig.ToParts();
            }
        });
    }

    public static void AddClangParseConfigs(IEnumerable<ISubconfig> subconfigs) {
        ClangParseConfigs.AddRange(subconfigs);
    }

    public static void AddLinkingConfigs(IEnumerable<ISubconfig> subconfigs) {
        LinkingConfigs.AddRange(subconfigs);
    }

    public static void AddObjectGenConfigs(IEnumerable<ISubconfig> subconfigs) {
        ObjectGenConfigs.AddRange(subconfigs);
    }

    public static void AddSubconfigCollection(SubconfigCollection collection) {
        AddClangParseConfigs(collection.ClangParseConfigs);
        AddLinkingConfigs(collection.LinkingConfigs);
        AddObjectGenConfigs(collection.ObjectGenConfigs);
    }

    public static void AddClangParseConfigs(IEnumerable<string> subconfigs) {
        ClangParseConfigs.Add(new ConstantSubconfig(subconfigs));
    }

    public static void AddLinkingConfigs(IEnumerable<string> subconfigs) {
        LinkingConfigs.Add(new ConstantSubconfig(subconfigs));
    }

    public static void AddObjectGenConfigs(IEnumerable<string> subconfigs) {
        ObjectGenConfigs.Add(new ConstantSubconfig(subconfigs));
    }

    public static IEnumerable<string> GetClangParseConfigs() {
        return ExpandSubconfigs(ClangParseConfigs)
            .Concat(["-D", "EPSL_PROJECT", "-I"+Utils.EPSLLIBS()]);
    }

    public static IEnumerable<string> GetLinkingConfigs() {
        IEnumerable<string> configs = ExpandSubconfigs(LinkingConfigs)
            .Concat(["--rtlib=compiler-rt"]);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            configs = configs.Concat(["-lm"]);
        }
        return configs;
    }

    public static IEnumerable<string> GetObjectGenConfigs() {
        return ExpandSubconfigs(ObjectGenConfigs);
    }

    public static SubconfigCollection ToSubconfigCollection() {
        return new SubconfigCollection(ClangParseConfigs, LinkingConfigs, ObjectGenConfigs);
    }
}
