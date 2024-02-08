using System;
using System.Collections.Generic;

public interface IFileCompiler {
    string GetText();
    List<string> ToImports();
    HashSet<string> ToStructIDs();
    void AddStructIDs(HashSet<string> structIds);
    List<Struct> ToStructs();
    void AddStructs(List<Struct> structs);
    List<RealFunctionDeclaration> ToDeclarations();
    void AddDeclarations(List<RealFunctionDeclaration> declarations);
    // TODO: change to` string ToExecutable();`
    string ToExecutable(string path);
}
