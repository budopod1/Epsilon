using System;
using System.Collections.Generic;

public class SubconfigCollection {
    public IEnumerable<ISubconfig> ClangParseConfigs;
    public IEnumerable<ISubconfig> LinkingConfigs;

    public SubconfigCollection(IEnumerable<ISubconfig> clangParseConfigs, IEnumerable<ISubconfig> linkingConfigs) {
        ClangParseConfigs = clangParseConfigs;
        LinkingConfigs = linkingConfigs;
    }

    public static SubconfigCollection Empty() {
        return new SubconfigCollection(new ISubconfig[0], new ISubconfig[0]);
    }
}
