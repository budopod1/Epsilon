using System;
using System.Collections.Generic;

public interface IFileCompiler {
    string GetText();
    List<string> ToImports();
    HashSet<LocatedID> ToStructIDs();
    void AddStructIDs(HashSet<LocatedID> structIds);
    List<RealFunctionDeclaration> ToDeclarations();
    void AddDeclarations(List<RealFunctionDeclaration> declarations);
    HashSet<Struct> ToStructs();
    void SetStructs(HashSet<Struct> structs);
    Dependencies ToDependencies(Func<string, FileTree> getFile);
    string ToIR(string suggestedPath);
    string GetSource();
    bool FromCache();
    bool ShouldSaveSPEC();
    IEnumerable<IClangConfig> GetClangConfig();
    FileSourceType GetFileSourceType();
}
