public class StringConstant(string value) : IConstant {
    readonly string value = value;

    public static readonly Dictionary<char, string> BackslashReplacements = new() {
        {'n', "\n"},
        {'t', "\t"},
        {'r', "\r"},
        {'\\', "\\"},
    };

    public static StringConstant FromString(string value) {
        return new StringConstant(JSONTools.FromLiteral(value));
    }

    public Type_ GetType_() {
        return Type_.String();
    }

    public string GetValue() {
        return value;
    }

    public bool IsTruthy() {
        return value.Length > 0;
    }

    public IJSONValue GetJSON() {
        throw new InvalidOperationException("String constants cannot appear in the final IR");
    }
}
