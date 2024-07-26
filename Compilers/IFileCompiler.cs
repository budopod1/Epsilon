using System;
using System.Collections.Generic;

public interface IFileCompiler {
    string GetText();
    string GetIDPath();
    IEnumerable<string> ToImports();
    SubconfigCollection GetSubconfigs();
    HashSet<LocatedID> ToStructIDs();
    void AddStructIDs(HashSet<LocatedID> structIds);
    List<RealFunctionDeclaration> ToDeclarations();
    void AddDeclarations(List<RealFunctionDeclaration> declarations);
    HashSet<Struct> ToStructs();
    void LoadStructExtendees();
    Dependencies ToDependencies(Builder builder);
    void FinishCompilation(string suggestedPath, bool recommendLLVM);
    string GetIR();
    string GetObj();
    string GetSource();
    bool FromCache();
    bool ShouldSaveSPEC();
    FileSourceType GetFileSourceType();
}
