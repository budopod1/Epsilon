using System;

public class LocatedID {
    public string Path;
    public string Name;

    public LocatedID(string path, string name) {
        Path = path;
        Name = name;
    }

    public string GetID() {
        return Name + " " + Path;
    }
}
