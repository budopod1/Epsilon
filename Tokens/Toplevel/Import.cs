using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class Import : ITopLevel {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    List<string> path;

    public Import(List<string> path) {
        this.path = path;
    }

    public List<string> GetPath() {
        return path;
    }

    public string GetRealPath() {
        return String.Join(
            Path.DirectorySeparatorChar,
            path.Select(part => part == "" ? ".." : part)
        );
    }
}
