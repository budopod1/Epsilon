using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public static class Utils {
    public static bool SUBPROCCESSOUTPUT = true;
    
    public static string Tab = "    ";
    public static string Whitespace = "\r\n\t ";
    public static string Numbers = "1234567890";
    public static string Uppercase = "QWERTYUIOPASDFGHJKLZXCVBNM";
    public static string Lowercase = "qwertyuiopasdfghjklzxcvbnm";
    public static string NameStartChars = Uppercase + Lowercase + "_";
    public static string NameChars = Uppercase + Lowercase + Numbers + "_";
    
    public static string WrapName(string name, string content, 
                                  string wrapStart="(", string wrapEnd=")") {
        return name + wrapStart + content + wrapEnd;
    }

    public static string WrapNewline(string text) {
        return "\n" + text + "\n";
    }

    public static string Indent(string text) {
        return Utils.Tab + text.Replace("\n", "\n" + Utils.Tab);
    }

    public static bool IsInstance(Type a, Type b) {
        if (b.IsAssignableFrom(a)) return true;
        if (a.IsGenericType)
            a = a.GetGenericTypeDefinition();
        if (b.IsGenericType)
            b = b.GetGenericTypeDefinition();
        if (a.IsSubclassOf(b)) return true;
        return a == b;
    }
    
    public static bool IsInstance(Object a, Type b) {
        return Utils.IsInstance(a.GetType(), b);
    }

    public static string TitleCase(string text) {
        return Char.ToUpper(text[0]) + text.Substring(1).ToLower();
    }

    public static bool ListEqual<T>(List<T> a, List<T> b) where T : IEquatable<T> {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++) {
            T ai = a[i];
            T bi = b[i];
            if (!ai.Equals(bi)) return false;
        }
        return true;
    }

    public static string CammelToSnake(string str) {
        string result = "";
        bool first = true;
        bool wasUpper = false;
        foreach (char chr in str) {
            bool isUpper = Char.IsUpper(chr);
            if (isUpper && !wasUpper && !first) result += "_";
            result += Char.ToLower(chr);
            wasUpper = isUpper;
            first = false;
        }
        return result;
    }

    public static string ProjectAbsolutePath() {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    public static bool ApproxEquals(double a, double b) {
        return Math.Abs(a-b)/(a+b) < 0.01;
    }

    public static Process RunCommand(string command, IEnumerable<string> arguments) {
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

    public static Process RunCommand(string command) {
        return RunCommand(command, new List<string>());
    }

    public static bool FileExists(string path) {
        try {
            FileStream file = File.Open(path, FileMode.Open);
            file.Dispose();
            return true;
        } catch (FileNotFoundException) {
            return false;
        } catch (DirectoryNotFoundException) {
            return false;
        } catch (UnauthorizedAccessException) {
            return false;
        } catch (ArgumentException) {
            return false;
        }
    }

    public static string GetFullPath(string path) {
        try {
            return Path.GetFullPath(path);
        } catch (ArgumentNullException) {
            return null;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string JoinPaths(params string[] segments) {
        try {
            return Path.Combine(segments);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string GetDirectoryName(string path) {
        try {
            return Path.GetDirectoryName(path);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string GetFileName(string path) {
        try {
            return Path.GetFileName(path);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string GetFileNameWithoutExtension(string path) {
        try {
            return Path.GetFileNameWithoutExtension(path);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string RemoveExtension(string path) {
        try {
            return Path.ChangeExtension(path, null);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string SetExtension(string path, string extension) {
        try {
            return Path.ChangeExtension(path, extension);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static bool TryDelete(string path) {
        try {
            File.Delete(path);
            return true;
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException) {
            return false;
        } catch (IOException) {
            return false;
        } catch (NotSupportedException) {
            return false;
        } catch (UnauthorizedAccessException) {
            return false;
        }
    }

    public static (int, int) LongToInts(long num) {
        return ((int)(num >> 32), (int)(num & ~(int)0));
    }

    public static long IntsToLong((int, int) vals) {
        (int a, int b) = vals;
        return ((long)a << 32) + (long)b;
    }
}
