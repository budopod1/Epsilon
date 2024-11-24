namespace Epsilon;
public static class EnumHelpers {
    public static CacheMode ParseCacheMode(string txt) {
        return txt.ToLower() switch {
            "dont-use" => CacheMode.DONTUSE,
            "dont-load" => CacheMode.DONTLOAD,
            "auto" => CacheMode.AUTO,
            "always" => CacheMode.ALWAYS,
            _ => throw new InvalidOperationException(),
        };
    }

    public static OutputType ParseOutputType(string txt) {
        return txt switch {
            "executable" => OutputType.EXECUTABLE,
            "llvm-ll" => OutputType.LLVMLL,
            "llvm-bc" => OutputType.LLVMBC,
            "package-both" => OutputType.PACKAGEBOTH,
            "package-obj" => OutputType.PACKAGEOBJ,
            "object" => OutputType.OBJECT,
            "shared-object" => OutputType.SHAREDOBJECT,
            _ => throw new InvalidOperationException(),
        };
    }

    public static string ParseOutputType(OutputType outputType) {
        return outputType switch {
            OutputType.NONE => "none",
            OutputType.EXECUTABLE => "executable",
            OutputType.LLVMLL => "llvm-ll",
            OutputType.LLVMBC => "llvm-bc",
            OutputType.PACKAGEBOTH => "package-both",
            OutputType.PACKAGEOBJ => "package-obj",
            OutputType.OBJECT => "object",
            OutputType.SHAREDOBJECT => "shared-object",
            _ => throw new InvalidOperationException(),
        };
    }

    public static string GetExtension(this OutputType outputType) {
        return outputType switch {
            OutputType.EXECUTABLE or OutputType.PACKAGEBOTH
            or OutputType.PACKAGEOBJ => null,
            OutputType.LLVMLL => "ll",
            OutputType.LLVMBC => "bc",
            OutputType.OBJECT => "o",
            OutputType.SHAREDOBJECT => "so",
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool DoesRequireLLVM(this OutputType outputType) {
        return outputType switch {
            OutputType.EXECUTABLE or OutputType.PACKAGEOBJ or OutputType.OBJECT => false,
            OutputType.LLVMLL or OutputType.LLVMBC or OutputType.PACKAGEBOTH
            or OutputType.SHAREDOBJECT => true,
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool MustntLinkBuiltins(this OutputType outputType) {
        return outputType switch {
            OutputType.EXECUTABLE or OutputType.LLVMLL or OutputType.LLVMBC
            or OutputType.OBJECT or OutputType.SHAREDOBJECT => false,
            OutputType.PACKAGEBOTH or OutputType.PACKAGEOBJ => true,
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool MustntLinkLibraries(this OutputType outputType) {
        return outputType switch {
            OutputType.EXECUTABLE or OutputType.LLVMLL or OutputType.LLVMBC
            or OutputType.OBJECT or OutputType.SHAREDOBJECT => false,
            OutputType.PACKAGEBOTH or OutputType.PACKAGEOBJ => true,
            _ => throw new InvalidOperationException(),
        };
    }

    public static bool MustntLinkBuiltinModules(this OutputType outputType) {
        return outputType switch {
            OutputType.EXECUTABLE or OutputType.LLVMLL or OutputType.LLVMBC
            or OutputType.OBJECT or OutputType.SHAREDOBJECT
            or OutputType.PACKAGEBOTH or OutputType.PACKAGEOBJ => false,
            _ => throw new InvalidOperationException(),
        };
    }

    public static OptimizationLevel ParseOptimizationLevel(string text) {
        if (int.TryParse(text, out int num)) {
            return (OptimizationLevel)num;
        }
        Enum.TryParse(text.ToUpper(), out OptimizationLevel result);
        return result;
    }
}
