using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public static class Utils {
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

    public static string SetExtension(string path, string extension) {
        try {
            return Path.ChangeExtension(path, extension);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }


    public static string RemoveExtension(string path) {
        return SetExtension(path, extension: null);
    }
    public static string GetExtension(string path) {
        string result;
        try {
            result = Path.GetExtension(path);
        } catch (ArgumentNullException e) {
            throw e;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
        if (result == "") return "";
        if (result[0] == '.') return result.Substring(1);
        return result;
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

    public static string[] GetFilesInDir(string dir) {
        try {
            return Directory.GetFiles(dir);
        } catch (UnauthorizedAccessException) {
            return new string[0];
        }
    }

    public static string TempDir() {
        return JoinPaths(ProjectAbsolutePath(), "temp");
    }

    public static string EPSLLIBS() {
        return JoinPaths(ProjectAbsolutePath(), "libs");
    }

    public static string Stem(string path) {
        string directory = GetDirectoryName(path);
        string name = GetFileNameWithoutExtension(path);
        if (name[0] == '.') name = name.Substring(1);
        return JoinPaths(directory, name);
    }

    public static (int?, int?) LongToInts(long? num) {
        if (num == null) return (null, null);
        return ((int)(num.Value >> 32), (int)(num.Value & ~(int)0));
    }

    public static long? IntsToLong((int?, int?) vals) {
        (int? a, int? b) = vals;
        if (a == null || b == null) return null;
        return ((long)a.Value << 32) + (long)b.Value;
    }
}
