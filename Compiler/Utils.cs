using System;
using System.Linq;
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

    public static Dictionary<TValue, TKey> InvertDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) {
        return dictionary.ToDictionary(val => val.Value, val => val.Key);
    }

    public static Dictionary<char, string> EscapeReplacements = new Dictionary<char, string> {
        {'\'', "\\'"},
        {'"', "\\\""},
        {'\n', "\\n"},
        {'\t', "\\t"},
        {'\r', "\\r"},
        {'\\', "\\\\"},
    };

    public static Dictionary<char, char> UnescapeReplacements = InvertDictionary(
        EscapeReplacements
    ).ToDictionary(val=>val.Key[1], val=>val.Value);

    public static string EscapeStringToLiteral(string str, char quote='"') {
        string result = quote.ToString();
        foreach (char chr in str) {
            if (EscapeReplacements.ContainsKey(chr)) {
                result += EscapeReplacements[chr];
            } else {
                result += chr;
            }
        }
        return result + quote;
    }

    public static string UnescapeStringFromLiteral(string str) {
        string quoteless = str.Substring(1, str.Length-2);
        string result = "";
        bool wbs = false;
        foreach (char chr in quoteless) {
            if (wbs) {
                if (UnescapeReplacements.ContainsKey(chr)) {
                    result += UnescapeReplacements[chr];
                } else {
                    result += chr;
                }
                wbs = false;
            } else {
                if (chr == '\\') {
                    wbs = true;
                } else {
                    result += chr;
                }
            }
        }
        return result;
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
