using System;
using System.Collections.Generic;

public class StringConstant : IConstant {
    string value;

    public static Dictionary<char, string> BackslashReplacements = new Dictionary<char, string> {
        {'n', "\n"},
        {'t', "\t"},
        {'r', "\r"},
        {'\\', "\\"},
    };

    public StringConstant(string value) {
        this.value = value;
    }

    public static StringConstant FromString(string value) {
        string quoteless = value.Substring(1, value.Length-2);
        string result = "";
        bool wbs = false;
        foreach (char chr in quoteless) {
            if (wbs) {
                result += BackslashReplacements[chr];
                wbs = false;
            } else {
                if (chr == '\\') {
                    wbs = true;
                } else {
                    result += chr;
                }
            }
        }
        return new StringConstant(result);
    }

    public Type_ GetType_() {
        throw new NotImplementedException("GetType_ is not implemented for StringConstant yet.");
    }
}
