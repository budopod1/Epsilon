using System;

public class Utils {
    public static string Tab = "    ";
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
        /*
        if (b.IsInterface()) {
            if (a.GetInterfaces().Contains(b)) return true;
        }
        */
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
}
