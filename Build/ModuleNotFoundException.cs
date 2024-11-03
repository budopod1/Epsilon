namespace Epsilon;
public class ModuleNotFoundException(string path) : Exception {
    public string Path = path;
}
