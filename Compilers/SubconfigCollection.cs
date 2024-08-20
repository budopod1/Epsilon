using System;
using System.Collections.Generic;

public class SubconfigCollection(IEnumerable<ISubconfig> clangParseConfigs, IEnumerable<ISubconfig> linkingConfigs) {
    public IEnumerable<ISubconfig> ClangParseConfigs = clangParseConfigs;
    public IEnumerable<ISubconfig> LinkingConfigs = linkingConfigs;

    public static SubconfigCollection Empty() {
        return new SubconfigCollection(new ISubconfig[0], new ISubconfig[0]);
    }
}
