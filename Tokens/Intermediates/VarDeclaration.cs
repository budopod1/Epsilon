namespace Epsilon;
public class VarDeclaration : IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly Type_ type_;
    readonly Name name;
    int id;

    public VarDeclaration(Type_ type_, Name name) {
        this.type_ = type_;
        this.name = name;
    }

    public VarDeclaration(Type_Token type_, Name name) {
        this.type_ = type_.GetValue();
        this.name = name;
    }

    public Name GetName() {
        return name;
    }

    public Type_ GetType_() {
        return type_;
    }

    public void SetID(int id) {
        this.id = id;
    }

    public int GetID() {
        return id;
    }

    public void Verify() {
        throw new SyntaxErrorException(
            "Unmatched variable declaration", this
        );
    }

    public override string ToString() {
        return $"VarDeclaration({type_}:{name.GetValue()})";
    }
}
