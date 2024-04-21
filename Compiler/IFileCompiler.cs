using System;
using System.Collections.Generic;

public interface IFileCompiler {
    string GetText();
    List<string> ToImports();
    HashSet<LocatedID> ToStructIDs();
    void AddStructIDs(HashSet<LocatedID> structIds);
    HashSet<Struct> ToStructs();
    void AddStructs(HashSet<Struct> structs);
    List<RealFunctionDeclaration> ToDeclarations();
    void AddDeclarations(List<RealFunctionDeclaration> declarations);
    string ToIR(string suggestedPath);
    string GetSource();
    bool ShouldSaveSPEC();
    IEnumerable<IClangConfig> GetClangConfig();
    FileSourceType GetFileSourceType();
}
