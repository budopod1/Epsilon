using System;

public class ModuleNotFoundException : Exception {
    public string Path;

    public ModuleNotFoundException(string path) {
        Path = path;
    }
}
