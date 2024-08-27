public class DispatchedFile(IFileCompiler compiler, string path, string oldCompilerPath = null, SPECFileCompiler oldCompiler = null, string generatedSPEC = null) {
    public IFileCompiler Compiler = compiler;
    public string OldCompilerPath = oldCompilerPath;
    public SPECFileCompiler OldCompiler = oldCompiler;
    public string Path = path;
    public string GeneratedSPEC = generatedSPEC;
}
