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
            string trueName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "runcommand.dll" : "runcommand.so";
            string path = Utils.JoinPaths(Utils.ProjectAbsolutePath(), "C-Run-Command", trueName);
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
        CRCProcessResult result = CRC_run_command(command, args, (uint)args.Length, 7);
        exitCode = result.status;

        string output = result.output;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            output = output.Replace("\r\n", "\n");
        }
        return output;
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

    public static string RunPython(string file, IEnumerable<string> args=null, bool ignoreErrors=false) {
        string cmd = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "py" : "python3";
        IEnumerable<string> pyArgs = [file, ..args??[]];
        if (ignoreErrors) {
            return RunCommand(cmd, pyArgs, out int _exitCode);
        } else {
            return RunCommand(cmd, pyArgs);
        }
    }

    public static string RunScript(string name, IEnumerable<string> args=null, bool ignoreErrors=false) {
        return RunPython(Utils.JoinPaths(Utils.ProjectAbsolutePath(), "scripts", name),
            args, ignoreErrors);
    }

    public static string RunLLVMTool(string name, IEnumerable<string> args=null, bool ignoreErrors=false) {
        return RunScript("mapLLVMcmd.py", [name, ..args??[]], ignoreErrors);
    }

    public static void LinkLLVM(IEnumerable<string> sources, string output, bool toLL=false) {
        List<string> args = toLL ? ["-S"] : [];
        args.AddRange(["-o", output, "--", ..sources]);
        RunLLVMTool("llvm-link", args);
    }

    public static void OptLLVM(string source, string output) {
        RunLLVMTool("opt", ["-O3", source, "-o", output]);
    }

    public static void LLVMToObj(string source, string output, bool positionIndependent=false) {
        List<string> args = [source, "-o", output, "-filetype=obj", "-O=0",
            ..Subconfigs.GetObjectGenConfigs()];
        if (positionIndependent) {
            args.Add("--relocation-model=pic");
        }
        RunLLVMTool("llc", args);
    }

    public static void ClangToExecutable(IEnumerable<string> sources, string output) {
        RunLLVMTool("clang", ["-no-pie", "-o", output, "-O0", ..Subconfigs.GetLinkingConfigs(),
            ..Subconfigs.GetObjectGenConfigs(), ..sources]);
    }

    public static void LLVMsToObj(List<string> sources, string output, bool positionIndependent=false) {
        string linkedIR = Utils.JoinPaths(Utils.TempDir(), "linked.bc");
        LinkLLVM(sources, linkedIR);
        LLVMToObj(linkedIR, output, positionIndependent);
    }

    public static void LinkObjsToObj(IEnumerable<string> sources, string output) {
        // This doesn't use Subconfigs.GetLinkingConfigs() because it isn't
        // linking to an executable
        RunScript("linkobjects.py", [output, ..sources]);
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
        RunScript("makeso.py", [output, ..Subconfigs.GetLinkingConfigs(),
            ..Subconfigs.GetObjectGenConfigs(), ..sources]);
    }

    public static IEnumerable<string> ListIncludeDirs(bool isCPP) {
        return RunScript("listincludedirs.py", [isCPP ? "c++" : "c"])
            .Trim().Split("\n");
    }

    public static string VerifyCSyntax(bool cpp, string file) {
        IEnumerable<string> args = new string[] {file, "-fsyntax-only"}
            .Concat(Subconfigs.GetClangParseConfigs());
        return RunLLVMTool(cpp ? "clang++" : "clang", args, ignoreErrors: true);
    }

    public static void CToLLVM(bool cpp, string from, string to_) {
        RunLLVMTool(cpp ? "clang++" : "clang", [from, "-o", to_,
            "-emit-llvm", "-c", ..Subconfigs.GetClangParseConfigs()]);
    }
}
