using System;
using System.Linq;
using System.Collections.Generic;

public class FileTree {
    public string PartialPath;
    public IFileCompiler Compiler;
    public SPECFileCompiler OldCompiler;
    public string GeneratedEPSLSPEC = null;
    public List<FileTree> Imported = new List<FileTree>();
    public List<string> Imports;
    public bool TreeLoaded = false;

    string _Text;
    public string Text { 
        get {
            if (_Text == null) _Text = Compiler.GetText();
            return _Text;
        }
        set {
            _Text = value;
        }
    }

    string _Path;
    public string Stemmed;
    public string Path { 
        get => _Path;
        set {
            _Path = value;
            Stemmed = Utils.Stem(value);
        }
    }

    public string OldText {
        get => OldCompiler.GetText();
    }

    EPSLSPEC _spec = null;
    public EPSLSPEC EPSLSPEC {
        // The version of C# I'm using doesn't have support for the ??= operator
        get => _spec = _spec ?? MakeSPEC();
    }

    public string OldPath;

    public FileSourceType SourceType;

    public HashSet<LocatedID> StructIDs;
    public HashSet<Struct> Structs;
    public List<RealFunctionDeclaration> Declarations;
    public Dependencies Dependencies;

    public HashSet<Struct> OldStructs;
    public List<RealFunctionDeclaration> OldDeclarations;

    public string IR;

    public FileTree(string partialPath, string path, IFileCompiler compiler, string oldCompilerPath, SPECFileCompiler oldCompiler, string generatedEPSLSPEC) {
        PartialPath = partialPath;
        Path = path;
        Compiler = compiler;
        OldPath = oldCompilerPath;
        SourceType = compiler.GetFileSourceType();
        OldCompiler = oldCompiler;
        Imports = compiler.ToImports();
        GeneratedEPSLSPEC = generatedEPSLSPEC;
    }

    EPSLSPEC MakeSPEC() {
        return new EPSLSPEC(
            Declarations, Structs, Dependencies, Compiler.GetClangConfig(),
            Imports, IR, Path, SourceType
        );
    }

    public string GetName() {
        string name = Utils.GetFileNameWithoutExtension(Path);
        if (name[0] == '.') return name.Substring(1);
        return name;
    }
}
