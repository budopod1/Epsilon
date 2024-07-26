using System;
using System.Linq;
using System.Collections.Generic;

public static class Subconfigs {
    static List<ISubconfig> ClangParseConfigs = new List<ISubconfig>();
    static List<ISubconfig> LinkingConfigs = new List<ISubconfig> {};

    static Dictionary<ISubconfig, IEnumerable<string>> cache = new Dictionary<ISubconfig, IEnumerable<string>>();

    static IEnumerable<string> ExpandSubconfigs(IEnumerable<ISubconfig> subconfigs) {
        return subconfigs.SelectMany(subconfig => {
            if (cache.ContainsKey(subconfig)) {
                return cache[subconfig];
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

    public static void AddSubconfigCollection(SubconfigCollection collection) {
        AddClangParseConfigs(collection.ClangParseConfigs);
        AddLinkingConfigs(collection.LinkingConfigs);
    }

    public static void AddClangParseConfigs(IEnumerable<string> subconfigs) {
        ClangParseConfigs.Add(new ConstantSubconfig(subconfigs));
    }

    public static void AddLinkingConfigs(IEnumerable<string> subconfigs) {
        LinkingConfigs.Add(new ConstantSubconfig(subconfigs));
    }

    public static IEnumerable<string> GetClangParseConfigs() {
        return ExpandSubconfigs(ClangParseConfigs);
    }

    public static IEnumerable<string> GetLinkingConfigs() {
        return ExpandSubconfigs(LinkingConfigs).Concat(new string[] {"-lc", "-lm"});
    }

    public static SubconfigCollection ToSubconfigCollection() {
        return new SubconfigCollection(ClangParseConfigs, LinkingConfigs);
    }
}
