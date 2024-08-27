public class Import(List<string> path) : ITopLevel {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly List<string> path = path;

    public List<string> GetPath() {
        return path;
    }

    public string GetRealPath() {
        return string.Join(
            Path.DirectorySeparatorChar,
            path.Select(part => part == "" ? ".." : part)
        );
    }
}
