using CsJSONTools;

public class FunctionArgumentToken(string name, Type_ type_, int id = -1) : IToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly string name = name;
    readonly Type_ type_ = type_;
    int id = id;

    public string GetName() {
        return name;
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
}
