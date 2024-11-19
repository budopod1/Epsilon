using System.Text;

namespace Epsilon;
public static class Utils {
    public static readonly string Tab = "    ";
    public static readonly string Whitespace = "\r\n\t ";
    public static readonly string Numbers = "1234567890";
    public static readonly string Uppercase = "QWERTYUIOPASDFGHJKLZXCVBNM";
    public static readonly string Lowercase = "qwertyuiopasdfghjklzxcvbnm";
    public static readonly string NameStartChars = Uppercase + Lowercase + "_";
    public static readonly string NameChars = Uppercase + Lowercase + Numbers + "_";

    public static string WrapName(string name, string content, string start="(", string end=")") {
        return name + start + content + end;
    }

    public static string WrapWithNewlines(string text) {
        return "\n" + text + "\n";
    }

    public static string Indent(string text) {
        return Tab + text.Replace("\n", "\n" + Tab);
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

    public static bool IsInstance(object a, Type b) {
        return IsInstance(a.GetType(), b);
    }

    public static string TitleCase(string text) {
        return char.ToUpper(text[0]) + text[1..].ToLower();
    }

    public static string CammelToSnake(string str) {
        StringBuilder result = new();
        bool first = true;
        bool wasUpper = false;
        foreach (char chr in str) {
            bool isUpper = char.IsUpper(chr);
            if (isUpper && !wasUpper && !first) result.Append('_');
            result.Append(char.ToLower(chr));
            wasUpper = isUpper;
            first = false;
        }
        return result.ToString();
    }

    public static string ProjectAbsolutePath() {
        DirectoryInfo dir = new(AppDomain.CurrentDomain.BaseDirectory);
        if (dir.Name != "bin") {
            throw new IOException("The Epsilon executable must be in a directory named 'bin'");
        }
        return dir.Parent.FullName;
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
        } catch (ArgumentNullException) {
            throw;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string GetDirectoryName(string path) {
        try {
            return Path.GetDirectoryName(path);
        } catch (ArgumentNullException) {
            throw;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string GetFileName(string path) {
        try {
            return Path.GetFileName(path);
        } catch (ArgumentNullException) {
            throw;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string GetFileNameWithoutExtension(string path) {
        try {
            return Path.GetFileNameWithoutExtension(path);
        } catch (ArgumentNullException) {
            throw;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
    }

    public static string SetExtension(string path, string extension) {
        try {
            return Path.ChangeExtension(path, extension);
        } catch (ArgumentNullException) {
            throw;
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
        } catch (ArgumentNullException) {
            throw;
        } catch (ArgumentException e) {
            throw new IOException(e.Message);
        }
        if (result == "") return "";
        if (result[0] == '.') return result[1..];
        return result;
    }

    public static bool TryDelete(string path) {
        try {
            File.Delete(path);
            return true;
        } catch (ArgumentNullException) {
            throw;
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

    public static void CreateFileUnlessExists(string path) {
        File.Open(path, FileMode.OpenOrCreate, FileAccess.Read).Close();
    }

    public static string[] GetFilesInDir(string dir) {
        try {
            return Directory.GetFiles(dir);
        } catch (UnauthorizedAccessException) {
            return [];
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
        if (name[0] == '.') name = name[1..];
        return JoinPaths(directory, name);
    }

    public static (int?, int?) LongToInts(long? num) {
        if (num == null) return (null, null);
        return ((int)(num.Value >> 32), (int)(num.Value & ~0));
    }

    public static long? IntsToLong((int?, int?) vals) {
        (int? a, int? b) = vals;
        if (a == null || b == null) return null;
        return ((long)a.Value << 32) + b.Value;
    }
}
