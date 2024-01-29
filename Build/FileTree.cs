using System;
using System.Collections.Generic;

public class FileTree {
    public string File;
    public IFileCompiler Compiler;
    public List<FileTree> Dependencies = new List<FileTree>();
    public List<string> DependencyPaths;

    public FileTree(string file, IFileCompiler compiler, List<string> dependencyPaths) {
        File = file;
        Compiler = compiler;
        DependencyPaths = dependencyPaths;
    }

    public IEnumerable<FileTree> IterTree() {
        yield return this;
        foreach (FileTree sub in Dependencies) {
            foreach (FileTree subsub in sub.IterTree()) {
                yield return subsub;
            }
        }
    }
}
