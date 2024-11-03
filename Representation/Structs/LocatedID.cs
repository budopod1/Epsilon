namespace Epsilon;
public class LocatedID(string path, string name) {
    public string Path = path;
    public string Name = name;

    public string GetID() {
        return Name + " " + Path;
    }

    public bool IsPrivate() {
        return Name.StartsWith('_');
    }
}
