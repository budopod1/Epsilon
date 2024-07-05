using System;
using System.Collections.Generic;

public interface IFileCompiler {
    string GetText();
    string GetIDPath();
    List<string> ToImports();
    HashSet<LocatedID> ToStructIDs();
    void AddStructIDs(HashSet<LocatedID> structIds);
    List<RealFunctionDeclaration> ToDeclarations();
    void AddDeclarations(List<RealFunctionDeclaration> declarations);
    HashSet<Struct> ToStructs();
    void LoadStructExtendees();
    Dependencies ToDependencies(Builder builder);
    string ToIR(string suggestedPath);
    string GetSource();
    bool FromCache();
    bool ShouldSaveSPEC();
    IEnumerable<IClangConfig> GetClangConfig();
    FileSourceType GetFileSourceType();
}
