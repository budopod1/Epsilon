using System;

public class DispatchedFile {
    public IFileCompiler Compiler;
    public string OldCompilerPath;
    public SPECFileCompiler OldCompiler;
    public string Path;
    public string GeneratedSPEC;

    public DispatchedFile(IFileCompiler compiler, string path, string oldCompilerPath=null, SPECFileCompiler oldCompiler=null, string generatedSPEC=null) {
        Compiler = compiler;
        OldCompilerPath = oldCompilerPath;
        OldCompiler = oldCompiler;
        Path = path;
        GeneratedSPEC = generatedSPEC;
    }
}
