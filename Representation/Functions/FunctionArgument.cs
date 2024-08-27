public class FunctionArgument : IEquatable<FunctionArgument> {
    readonly string name;
    readonly Type_ type_;
    readonly bool exactType_Match;
    int id;

    public FunctionArgument(string name, Type_ type_, bool exactType_Match=false, int id = -1) {
        this.name = name;
        this.type_ = type_;
        this.exactType_Match = exactType_Match;
        this.id = id;
    }

    public FunctionArgument(FunctionArgumentToken token) {
        name = token.GetName();
        type_ = token.GetType_();
        exactType_Match = false;
        id = token.GetID();
    }

    public string GetName() {
        return name;
    }

    public bool IsCompatibleWith(Type_ other) {
        if (exactType_Match) {
            return other.Matches(type_);
        } else {
            return other.IsConvertibleTo(type_);
        }
    }

    public Type_ GetType_() {
        return type_;
    }

    public int GetID() {
        return id;
    }

    public void SetID(int id) {
        this.id = id;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, type_.ToString() + ":" + name);
    }

    public IJSONValue GetJSON() {
        return new JSONObject {
            ["name"] = new JSONString(name),
            ["type_"] = type_.GetJSON(),
            ["variable"] = new JSONInt(id)
        };
    }

    public bool Equals(FunctionArgument other) {
        if (name != other.GetName()) return false;
        return type_.Equals(other.GetType_());
    }
}
