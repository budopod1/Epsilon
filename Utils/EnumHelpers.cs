using System;

public static class EnumHelpers {
    public static CacheMode ParseCacheMode(string txt) {
        switch (txt) {
        case "dont-use":
            return CacheMode.DONTUSE;
        case "dont-load":
            return CacheMode.DONTLOAD;
        case "auto":
            return CacheMode.AUTO;
        case "always":
            return CacheMode.ALWAYS;
        default:
            throw new InvalidOperationException();
        }
    }

    public static OutputType ParseOutputType(string txt) {
        switch (txt) {
        case "executable":
            return OutputType.EXECUTABLE;
        case "llvm-ll":
            return OutputType.LLVMLL;
        case "llvm-bc":
            return OutputType.LLVMBC;
        case "package-both":
            return OutputType.PACKAGEBOTH;
        case "package-obj":
            return OutputType.PACKAGEOBJ;
        case "object":
            return OutputType.OBJECT;
        case "shared-object":
            return OutputType.SHAREDOBJECT;
        default:
            throw new InvalidOperationException();
        }
    }

    public static string ParseOutputType(OutputType outputType) {
        switch (outputType) {
        case OutputType.NONE:
            return "none";
        case OutputType.EXECUTABLE:
            return "executable";
        case OutputType.LLVMLL:
            return "llvm-ll";
        case OutputType.LLVMBC:
            return "llvm-bc";
        case OutputType.PACKAGEBOTH:
            return "package-both";
        case OutputType.PACKAGEOBJ:
            return "package-obj";
        case OutputType.OBJECT:
            return "object";
        case OutputType.SHAREDOBJECT:
            return "shared-object";
        default:
            throw new InvalidOperationException();
        }
    }

    public static string GetExtension(this OutputType outputType) {
        switch (outputType) {
        case OutputType.EXECUTABLE:
        case OutputType.PACKAGEBOTH:
        case OutputType.PACKAGEOBJ:
            return null;
        case OutputType.LLVMLL:
            return "ll";
        case OutputType.LLVMBC:
            return "bc";
        case OutputType.OBJECT:
            return "o";
        case OutputType.SHAREDOBJECT:
            return "so";
        default:
            throw new InvalidOperationException();
        }
    }

    public static bool DoesRequireLLVM(this OutputType outputType) {
        switch (outputType) {
        case OutputType.EXECUTABLE:
        case OutputType.PACKAGEOBJ:
        case OutputType.OBJECT:
            return false;
        case OutputType.LLVMLL:
        case OutputType.LLVMBC:
        case OutputType.PACKAGEBOTH:
        case OutputType.SHAREDOBJECT:
            return true;
        default:
            throw new InvalidOperationException();
        }
    }

    public static bool MustntLinkBuiltins(this OutputType outputType) {
        switch (outputType) {
        case OutputType.EXECUTABLE:
        case OutputType.LLVMLL:
        case OutputType.LLVMBC:
        case OutputType.OBJECT:
        case OutputType.SHAREDOBJECT:
            return false;
        case OutputType.PACKAGEBOTH:
        case OutputType.PACKAGEOBJ:
            return true;
        default:
            throw new InvalidOperationException();
        }
    }

    public static bool MustntLinkLibraries(this OutputType outputType) {
        switch (outputType) {
        case OutputType.EXECUTABLE:
        case OutputType.LLVMLL:
        case OutputType.LLVMBC:
        case OutputType.OBJECT:
        case OutputType.SHAREDOBJECT:
            return false;
        case OutputType.PACKAGEBOTH:
        case OutputType.PACKAGEOBJ:
            return true;
        default:
            throw new InvalidOperationException();
        }
    }

    public static OptimizationLevel ParseOptimizationLevel(string text) {
        if (Int32.TryParse(text, out int num)) {
            return (OptimizationLevel)num;
        }
        Enum.TryParse(text, out OptimizationLevel result);
        return result;
    }
}
