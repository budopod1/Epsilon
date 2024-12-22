using System.Reflection;
using System.Runtime.InteropServices;

namespace Epsilon;
public static class CmdUtils {
    static readonly bool SUBPROCCESSOUTPUT = false;

    static bool HasSetDllResolver = false;

    #pragma warning disable CS0649
    struct CRCProcessResult {
        public string output;
        public string error;
        public byte status;
    }

    [DllImport("runcommand.so")]
    static extern CRCProcessResult CRC_run_command(string prog, string[] args, uint argCount, uint captureMode);

    static IntPtr DllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath) {
        if (libraryName == "runcommand.so") {
            string path = Utils.JoinPaths(Utils.ProjectAbsolutePath(), "C-Run-Command", "runcommand.so");
            return NativeLibrary.Load(path, assembly, searchPath);
        }

        return IntPtr.Zero;
    }

    static string RunCommand(string command, IEnumerable<string> arguments, out int exitCode) {
        if (!HasSetDllResolver) {
            NativeLibrary.SetDllImportResolver(
                Assembly.GetExecutingAssembly(), DllImportResolver);
            HasSetDllResolver = true;
        }

        string[] args = arguments.ToArray();
        Log.Info(command, $"[{string.Join(", ", arguments)}]");
        CRCProcessResult result = CRC_run_command(command, args, (uint)args.Length, 1);
        exitCode = result.status;
        return result.output;
    }

    static string RunCommand(string command, IEnumerable<string> arguments) {
        string result = RunCommand(command, arguments, out int exitCode);
        if (SUBPROCCESSOUTPUT || exitCode != 0) {
            Console.WriteLine(result);
        }
        if (exitCode != 0) {
            throw new CommandFailureException(
                $"Command '{command}' exited with status code {exitCode}"
            );
        }
        return result;
    }

    public static void LinkLLVM(IEnumerable<string> sources, string output, bool toLL=false) {
        List<string> args = toLL ? ["-S"] : [];
        args.AddRange(["-o", output, "--", ..sources]);
        RunCommand("llvm-link", args);
    }

    public static void OptLLVM(string source, string output) {
        RunCommand("opt", ["-O3", source, "-o", output]);
    }

    public static void LLVMToObj(string source, string output, bool positionIndependent=false) {
        List<string> args = [source, "-o", output, "-filetype=obj", "-O=0",
            ..Subconfigs.GetObjectGenConfigs()];
        if (positionIndependent) {
            args.Add("--relocation-model=pic");
        }
        RunCommand("llc", args);
    }

    public static void ClangToExecutable(IEnumerable<string> sources, string output) {
        List<string> args = ["-no-pie", "-o", output, "-O0", ..Subconfigs.GetLinkingConfigs(),
            ..Subconfigs.GetObjectGenConfigs(), ..sources];
        RunCommand("clang", args);
    }

    public static void LLVMsToObj(List<string> sources, string output, bool positionIndependent=false) {
        string linkedIR = Utils.JoinPaths(Utils.TempDir(), "linked.bc");
        LinkLLVM(sources, linkedIR);
        LLVMToObj(linkedIR, output, positionIndependent);
    }

    public static string RunScript(string name, IEnumerable<string> args=null) {
        IEnumerable<string> bashArgs = ["--", Utils.JoinPaths(Utils.ProjectAbsolutePath(), name)];
        if (args != null) bashArgs = bashArgs.Concat(args);
        return RunCommand("bash", bashArgs);
    }

    public static void LinkObjsToObj(IEnumerable<string> sources, string output) {
        // This doesn't use Subconfigs.GetLinkingConfigs() because it isn't
        // linking to an executable
        RunCommand("ld", new string[] {"-r", "-o", output}.Concat(sources));
    }

    public static void FilesToObject(IEnumerable<string> sources, string output) {
        List<string> objs = [];
        List<string> llvm = [];

        foreach (string source in sources) {
            switch (Utils.GetExtension(source)) {
            case "ll":
            case "bc":
                llvm.Add(source);
                break;
            case "o":
                objs.Add(source);
                break;
            default:
                throw new ArgumentException(
                    $"{source} can't be converted into a .o file via CmdUtils.FilesToObject"
                );
            }
        }

        if (llvm.Count == 0) {
            LinkObjsToObj(objs, output);
        } else if (objs.Count == 0) {
            LLVMsToObj(llvm, output);
        } else {
            string llvmObj = Utils.JoinPaths(Utils.TempDir(), "ir.o");
            LLVMsToObj(llvm, llvmObj);
            objs.Add(llvmObj);
            LinkObjsToObj(objs, output);
        }
    }

    public static void ToSharedObject(IEnumerable<string> sources, string output) {
        RunCommand("clang", ["-shared", "-fPIC", "-o", output, ..Subconfigs.GetLinkingConfigs(),
            ..Subconfigs.GetObjectGenConfigs(), ..sources]);
    }

    public static IEnumerable<string> ListIncludeDirs(bool isCPP) {
        List<string> includeDirs = [];
        string raw = RunCommand("cpp", ["--verbose", "-x", isCPP ? "c++" : "c", "/dev/null"]);
        bool isSearchList = false;
        foreach (string line in raw.Split("\n")) {
            if (isSearchList) {
                if (line == "End of search list.") {
                    isSearchList = false;
                } else if (line.Length > 0 && line[0] == ' ') {
                    includeDirs.Add(line[1..]);
                }
            } else if (line == "#include <...> search starts here:") {
                isSearchList = true;
            }
        }
        return includeDirs;
    }

    public static string VerifyCSyntax(bool cpp, string file) {
        IEnumerable<string> args = new string[] {file, "-fsyntax-only"}
            .Concat(Subconfigs.GetClangParseConfigs());
        return RunCommand(cpp ? "clang++" : "clang", args, out int exitCode);
    }

    public static void CToLLVM(bool cpp, string from, string to_) {
        RunCommand(cpp ? "clang++" : "clang", [from, "-o", to_, "-emit-llvm", "-c",
            ..Subconfigs.GetClangParseConfigs()]);
    }
}
