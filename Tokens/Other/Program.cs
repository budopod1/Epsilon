public class Program : TreeToken, IVerifier, IHasScope {
    readonly string path;
    readonly HashSet<LocatedID> structIds = [];
    readonly IDCounter functionIDCounter = new();
    readonly IDCounter scopeVarIDCounter = new();
    HashSet<Struct> structsHere = [];
    readonly List<(CodeSpan, Type_)> parsedTypes_ = [];
    readonly List<RealFunctionDeclaration> externalDeclarations = [];
    readonly IScope scope;

    public Program(string path, List<IToken> tokens) : base(tokens) {
        this.path = path;
        scope = new Scope(scopeVarIDCounter);
    }

    public Program(string path, List<IToken> tokens, HashSet<LocatedID> structIds, IDCounter functionIDCounter, IDCounter scopeVarIDCounter, HashSet<Struct> structsHere, List<(CodeSpan, Type_)> parsedTypes_, List<RealFunctionDeclaration> externalDeclarations, IScope scope) : base(tokens) {
        this.path = path;
        this.structIds = structIds;
        this.functionIDCounter = functionIDCounter;
        this.scopeVarIDCounter = scopeVarIDCounter;
        this.structsHere = structsHere;
        this.parsedTypes_ = parsedTypes_;
        this.externalDeclarations = externalDeclarations;
        this.scope = scope;
    }

    public HashSet<LocatedID> GetStructIDs() {
        return structIds;
    }

    public void UpdateParents() {
        TokenUtils.UpdateParents(this);
        parent = null;
    }

    public void AddStructIDs(HashSet<LocatedID> structIds) {
        this.structIds.UnionWith(structIds);
    }

    public int GetFunctionID() {
        return functionIDCounter.GetID();
    }

    public IDCounter GetScopeVarIDCounter() {
        return scopeVarIDCounter;
    }

    public void SetStructsHere(HashSet<Struct> structs) {
        structsHere = structs;
    }

    public HashSet<Struct> GetStructsHere() {
        return structsHere;
    }

    public void AddParsedType_(CodeSpan span, Type_ type_) {
        parsedTypes_.Add((span, type_));
    }

    public IEnumerable<(CodeSpan, Type_)> ListParsedTypes_() {
        return parsedTypes_;
    }

    public void AddExternalDeclarations(List<RealFunctionDeclaration> declarations) {
        externalDeclarations.AddRange(declarations);
    }

    public List<RealFunctionDeclaration> GetExternalDeclarations() {
        return externalDeclarations;
    }

    public IScope GetScope() {
        return scope;
    }

    public void AddGlobals(IEnumerable<Global> globals) {
        foreach (Global global_ in globals) {
            scope.AddVar(global_.GetName(), global_.GetType_());
        }
    }

    protected override TreeToken _Copy(List<IToken> tokens) {
        return new Program(path, tokens, structIds, functionIDCounter, scopeVarIDCounter, structsHere, parsedTypes_, externalDeclarations, scope);
    }

    public void Verify() {
        bool foundMain = false;
        foreach (IToken token in this) {
            if (token is not ITopLevel) {
                throw new SyntaxErrorException(
                    "Invalid toplevel syntax", token
                );
            }
            if (token is Function func) {
                if (func.IsMain()) {
                    if (foundMain) {
                        throw new SyntaxErrorException(
                            "Only one main function can be defined", func
                        );
                    }
                    foundMain = true;
                }
            }
        }
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new() {
            ["functions"] = new JSONList(this.OfType<Function>().Select(
                function => function.GetFullJSON()
            )),
            ["extern_functions"] = new JSONList(
                externalDeclarations.Select(declaration => declaration.GetJSON())
            ),
            ["structs"] = new JSONList(StructsCtx.Structs().Select(struct_ => struct_.GetJSON())),
            ["id_path"] = new JSONString(path),
            ["globals"] = new JSONList(Scope.GetVarsJSON(scope))
        };
        return obj;
    }
}
