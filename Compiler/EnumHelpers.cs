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
        default:
            throw new InvalidOperationException();
        }
    }

    public static string GetExtension(this OutputType outputType) {
        switch (outputType) {
        case OutputType.EXECUTABLE:
            return null;
        case OutputType.LLVMLL:
            return "ll";
        case OutputType.LLVMBC:
            return "bc";
        default:
            throw new InvalidOperationException();
        }
    }

    public static bool DoesRequireLLVM(this OutputType outputType) {
        switch (outputType) {
        case OutputType.EXECUTABLE:
            return false;
        case OutputType.LLVMLL:
            return true;
        case OutputType.LLVMBC:
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
