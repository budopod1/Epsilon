using System;
using System.Collections.Generic;

public interface IFileCompiler {
    string GetText();
    List<string> ToImports();
    HashSet<string> ToBaseTypes_();
    void AddBaseTypes_(HashSet<string> baseTypes_);
    List<Struct> ToStructs();
    void AddStructs(List<Struct> structs);
    List<RealFunctionDeclaration> ToDeclarations();
    void AddDeclarations(List<RealFunctionDeclaration> declarations);
    string ToExecutable(string path);
}
