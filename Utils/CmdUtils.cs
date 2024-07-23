using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class CmdUtils {
    static readonly string[] baseClangArgs = new string[] {"-lc", "-lm"};
    static readonly bool SUBPROCCESSOUTPUT = false;

    #pragma warning disable CS0649
    struct _ProcessResult {
        public string output;
        public byte status;
    }

    [DllImport("./runcommand.so")]
    static extern _ProcessResult _RunCommand(string prog, string[] args, int argCount);

    static string RunCommand(string command, IEnumerable<string> arguments, out int exitCode) {
        string[] args = arguments.ToArray();
        _ProcessResult result = _RunCommand(command, args, args.Length);
        exitCode = result.status;
        if (SUBPROCCESSOUTPUT || exitCode != 0) {
            Console.Write(result.output);
        }
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
        List<string> args = new List<string> {"-o", output};
        if (toLL) {
            args.Add("-S");
        }
        args.Add("--");
        args.AddRange(sources);
        RunCommand("llvm-link", args);
    }

    public static void OptLLVM(string source, string output) {
        RunCommand("opt", new string[] {
            "-O3", source, "-o", output
        });
    }

    public static void LLVMToObj(string source, string output, bool positionIndependent=false) {
        List<string> args = new List<string> {source, "-o", output, "-filetype=obj"};
        if (positionIndependent) {
            args.Add("--relocation-model=pic");
        }
        RunCommand("llc", args);
    }

    public static IEnumerable<string> ConfigsToStrs(IEnumerable<IClangConfig> configs) {
        return configs.SelectMany(config => config.ToParts()).Concat(baseClangArgs);
    }

    public static void ClangToExecutable(IEnumerable<string> sources, string output, IEnumerable<IClangConfig> configs) {
        List<string> args = ConfigsToStrs(configs).Concat(new string[] {"-o", output})
            .Concat(sources).ToList();
        RunCommand("clang", args);
    }

    public static void LLVMsToObj(List<string> sources, string output, bool positionIndependent=false) {
        string linkedIR = Utils.JoinPaths(Utils.TempDir(), "linked.bc");
        LinkLLVM(sources, linkedIR);
        LLVMToObj(linkedIR, output, positionIndependent);
    }

    public static string RunScript(string name, IEnumerable<string> args=null) {
        IEnumerable<string> bashArgs = new string[] {
            "--", Utils.JoinPaths(Utils.ProjectAbsolutePath(), name)};
        if (args != null) bashArgs = bashArgs.Concat(args);
        return RunCommand("bash", bashArgs);
    }

    public static void LinkObjsToObj(IEnumerable<string> sources, string output) {
        RunCommand("ld", new string[] {"-r", "-o", output}.Concat(sources));
    }

    public static void FilesToObject(IEnumerable<string> sources, string output) {
        List<string> objs = new List<string>();
        List<string> llvm = new List<string>();

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
        RunCommand("clang", baseClangArgs.Concat(new string[] {"-shared", "-o", output})
            .Concat(sources));
    }

    public static IEnumerable<string> ListIncludeDirs() {
        string raw = RunCommand("cpp", new string[] {"--verbose", "/dev/null"});
        bool isSearchList = false;
        foreach (string line in raw.Split("\n")) {
            if (isSearchList) {
                if (line == "End of search list.") {
                    isSearchList = false;
                } else if (line.Length > 0 && line[0] == ' ') {
                    yield return line.Substring(1);
                }
            } else if (line == "#include <...> search starts here:") {
                isSearchList = true;
            }
        }
    }

    public static string VerifyCSyntax(string file, IEnumerable<string> config) {
        return RunCommand("clang", new string[] {file, "-fsyntax-only"}.Concat(config), out int exitCode);
    }

    public static void CToLLVM(string from, string to_, IEnumerable<string> config) {
        RunCommand("clang", new string[] {from, "-o", to_, "-emit-llvm", "-c"}.Concat(config));
    }

    public static void CToObj(string from, string to_, IEnumerable<string> config) {
        RunCommand("clang", new string[] {from, "-o", to_, "-c"}.Concat(config));
    }
}
