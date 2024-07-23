using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class FileTree {
    public string PartialPath;
    public IFileCompiler Compiler;
    public SPECFileCompiler OldCompiler;
    public string GeneratedEPSLSPEC = null;
    public List<FileTree> Imported = new List<FileTree>();
    public IEnumerable<string> Imports;
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

    string _Path_;
    public string Stemmed;
    public string Path_ {
        get => _Path_;
        set {
            _Path_ = value;
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

    string _idPath = null;
    public string IDPath {
        get => _idPath = _idPath ?? Compiler.GetIDPath();
    }

    public string OldPath;

    public FileSourceType SourceType;

    public HashSet<LocatedID> StructIDs;
    public HashSet<Struct> Structs;
    public List<RealFunctionDeclaration> Declarations;
    public Dependencies Dependencies;

    public HashSet<Struct> OldStructs;
    public List<RealFunctionDeclaration> OldDeclarations;

    public bool IsUnlinked = false;

    public string SuggestedIntermediatePath;

    public string IR;
    public string Obj;

    public bool IRIsInUserDir {
        get => IR != null && SuggestedIntermediatePath == Utils.RemoveExtension(IR);
    }

    public bool ObjIsInUserDir {
        get => Obj != null && SuggestedIntermediatePath == Utils.RemoveExtension(Obj);
    }

    public IntermediateFile Intermediate;

    public FileTree(string partialPath, string path, IFileCompiler compiler, string oldCompilerPath, SPECFileCompiler oldCompiler, string generatedEPSLSPEC) {
        PartialPath = partialPath;
        Path_ = path;
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
            Imports, IR, Obj, Path_, SourceType, IDPath
        );
    }

    public string GetName() {
        string name = Utils.GetFileNameWithoutExtension(Path_);
        if (name[0] == '.') return name.Substring(1);
        return name;
    }
}
