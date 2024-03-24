using System;

public class DispatchedFile {
    public IFileCompiler Compiler;
    public string Path;
    public string GeneratedSPEC;

    public DispatchedFile(IFileCompiler compiler, string path, string generatedSPEC=null) {
        Compiler = compiler;
        Path = path;
        GeneratedSPEC = generatedSPEC;
    }
}
