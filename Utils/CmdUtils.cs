using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

public static class CmdUtils {
    static readonly bool SUBPROCCESSOUTPUT = false;
    
    static Process RunCommand(string command, IEnumerable<string> arguments) {
        ProcessStartInfo startInfo = new ProcessStartInfo(command);
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        string argumentsStr = "";
        // https://stackoverflow.com/questions/5510343
        foreach (string argument in arguments) {
            argumentsStr += "\"" + argument.Replace("\\", "\\\\")
                .Replace("\"", "\\\"") + "\" ";
        }
        Log.Info(command, argumentsStr);
        startInfo.Arguments = argumentsStr;
        Process process = Process.Start(startInfo);
        process.WaitForExit();
        if (SUBPROCCESSOUTPUT || process.ExitCode != 0) {
            Console.Write(process.StandardOutput.ReadToEnd());
            Console.Write(process.StandardError.ReadToEnd());
        }
        if (process.ExitCode != 0) {
            throw new CommandFailureException(
                $"Command '{command}' exited with status code {process.ExitCode}"
            );
        }
        return process;
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

    public static void ClangToExecutable(IEnumerable<string> sources, string output, IEnumerable<IClangConfig> configs, bool positionIndependent=false) {
        // List<string> args = configs.Select(config => config.ToString()).ToList();
        List<string> args = new List<string> {"-lc", "-lm", "-o", output};
        if (positionIndependent) {
            args.Add("-fPIC");
        }
        args.AddRange(sources);
        RunCommand("clang", args);
    }

    public static void LLVMsToObj(List<string> sources, string output) {
        RunCommand("clang", new string[] {"-c", "-o", output}.Concat(sources));
    }

    public static void RunScript(string name) {
        RunCommand("bash", new string[] {
            "--", Utils.JoinPaths(Utils.ProjectAbsolutePath(), name)});
    }

    public static void LinkObjsToObj(IEnumerable<string> sources, string output) {
        RunCommand("ld", new string[] {"-r", "-o", output}.Concat(sources));
    }

    public static void FilesToObject(IEnumerable<string> sources, string output) {
        List<string> objs = new List<string>();
        List<string> llvm = new List<string>();
        
        foreach (string source in sources) {
            switch (Utils.GetExtension(source)) {
            case ".ll":
            case ".bc":
                llvm.Add(source);
                break;
            case ".o":
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
}
