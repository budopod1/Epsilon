public class FunctionCall(FunctionDeclaration function, List<IValueToken> arguments) : IFunctionCall, IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly FunctionDeclaration function = function;
    readonly List<IValueToken> arguments = arguments;

    public int Count {
        get {
            return arguments.Count;
        }
    }

    public IToken this[int i] {
        get {
            return arguments[i];
        }
        set {
            arguments[i] = (IValueToken)value;
        }
    }

    public FunctionDeclaration GetFunction() {
        return function;
    }

    public Type_ GetType_() {
        return function.GetReturnType_(arguments);
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, string.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        ));
    }

    public int Serialize(SerializationContext context) {
        return new SerializableInstruction(context, this) {
            ["function"] = function.GetID()
        }.Register();
    }
}
