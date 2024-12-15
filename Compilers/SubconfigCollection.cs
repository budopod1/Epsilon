namespace Epsilon;
public class SubconfigCollection(IEnumerable<ISubconfig> clangParseConfigs, IEnumerable<ISubconfig> linkingConfigs, IEnumerable<ISubconfig> objectGenConfigs) {
    public IEnumerable<ISubconfig> ClangParseConfigs = clangParseConfigs;
    public IEnumerable<ISubconfig> LinkingConfigs = linkingConfigs;
    public IEnumerable<ISubconfig> ObjectGenConfigs = objectGenConfigs;

    public static SubconfigCollection Empty() {
        return new SubconfigCollection([], [], []);
    }
}
