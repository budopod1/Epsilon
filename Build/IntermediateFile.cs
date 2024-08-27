public class IntermediateFile(IntermediateFile.IntermediateType fileType, string path, bool isInUserDir) {
    public enum IntermediateType {
        IR,
        Obj
    }

    public IntermediateType FileType = fileType;
    public string Path = path;
    public bool IsInUserDir = isInUserDir;

    public static IntermediateFile FromFileTree(FileTree file, bool shouldBeIR) {
        var IRType = IntermediateType.IR;
        var ObjType = IntermediateType.Obj;
        if (shouldBeIR) {
            if (file.IR != null) {
                return new IntermediateFile(IRType, file.IR, file.IRIsInUserDir);
            } else if (file.Obj != null) {
                return new IntermediateFile(ObjType, file.Obj, file.ObjIsInUserDir);
            }
        } else {
            if (file.Obj != null) {
                return new IntermediateFile(ObjType, file.Obj, file.ObjIsInUserDir);
            } else if (file.IR != null) {
                return new IntermediateFile(IRType, file.IR, file.IRIsInUserDir);
            }
        }
        throw new InvalidOperationException("IFileCompiler does not have an intermediate");
    }
}
