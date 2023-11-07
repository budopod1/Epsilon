using System;
using System.Collections.Generic;

public class Utils {
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

    public static Dictionary<char, string> LiteralReplacements = new Dictionary<char, string> {
        {'\n', "\\n"},
        {'\t', "\\t"},
        {'\r', "\\r"},
        {'\\', "\\\\"},
    };

    public static string EscapeStringToLiteral(string str) {
        string result = "\"";
        foreach (char chr in str) {
            if (LiteralReplacements.ContainsKey(chr)) {
                result += LiteralReplacements[chr];
            } else {
                result += chr;
            }
        }
        return result + "\"";
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
}
