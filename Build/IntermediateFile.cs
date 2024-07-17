using System;

public class IntermediateFile {
    public enum IntermediateType {
        IR,
        Obj
    }

    public IntermediateType FileType;
    public string Path;
    public bool IsInUserDir;

    public IntermediateFile(IntermediateType fileType, string path, bool isInUserDir) {
        FileType = fileType;
        Path = path;
        IsInUserDir = isInUserDir;
    }

    public static IntermediateFile FromFileTree(FileTree file, bool shouldBeIR) {
        var IRType = IntermediateFile.IntermediateType.IR;
        var ObjType = IntermediateFile.IntermediateType.Obj;
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
