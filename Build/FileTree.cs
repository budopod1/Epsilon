using System;
using System.Collections.Generic;

public class FileTree {
    public string File;
    public IFileCompiler Compiler;
    public List<FileTree> Dependencies = new List<FileTree>();
    public List<string> DependencyPaths;
    public bool TreeLoaded = false;

    public HashSet<LocatedID> StructIDs;
    public List<Struct> Structs;
    public List<RealFunctionDeclaration> Declarations;

    public FileTree(string file, IFileCompiler compiler, List<string> dependencyPaths) {
        File = file;
        Compiler = compiler;
        DependencyPaths = dependencyPaths;
    }
}
