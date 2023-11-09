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
        return new StringConstant(Utils.UnescapeStringFromLiteral(value));
    }

    public Type_ GetType_() {
        throw new NotImplementedException("GetType_ is not implemented for StringConstant yet.");
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["type"] = new JSONString("string");
        obj["value"] = new JSONString(value);
        return obj;
    }
}
