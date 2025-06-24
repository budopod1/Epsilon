using CsJSONTools;

namespace Epsilon;
public class Program : TreeToken, IVerifier, IHasScope {
    readonly string realPath;
    readonly string idPath;
    readonly FileExerptManager exerptManager;
    readonly BuildSettings buildSettings;
    readonly HashSet<LocatedID> structIds = [];
    readonly IDCounter functionIDCounter = new();
    readonly IDCounter scopeVarIDCounter = new();
    HashSet<Struct> structsHere = [];
    readonly List<(CodeSpan, Type_)> parsedTypes_ = [];
    readonly List<RealFunctionDeclaration> externalDeclarations = [];
    readonly IScope scope;

    public Program(string realPath, string idPath, FileExerptManager exerptManager,
            List<IToken> tokens, BuildSettings buildSettings) : base(tokens) {
        this.realPath = realPath;
        this.idPath = idPath;
        this.exerptManager = exerptManager;
        this.buildSettings = buildSettings;
        scope = new Scope(scopeVarIDCounter);
    }

    public Program(string realPath, string idPath, FileExerptManager exerptManager, List<IToken> tokens,
            BuildSettings buildSettings, HashSet<LocatedID> structIds, IDCounter functionIDCounter,
            IDCounter scopeVarIDCounter, HashSet<Struct> structsHere,
            List<(CodeSpan, Type_)> parsedTypes_,
            List<RealFunctionDeclaration> externalDeclarations, IScope scope) : base(tokens) {
        this.realPath = realPath;
        this.idPath = idPath;
        this.exerptManager = exerptManager;
        this.buildSettings = buildSettings;
        this.structIds = structIds;
        this.functionIDCounter = functionIDCounter;
        this.scopeVarIDCounter = scopeVarIDCounter;
        this.structsHere = structsHere;
        this.parsedTypes_ = parsedTypes_;
        this.externalDeclarations = externalDeclarations;
        this.scope = scope;
    }

    public string GetRealPath() {
        return realPath;
    }

    public FileExerptManager GetExerptManager() {
        return exerptManager;
    }

    public BuildSettings GetBuildSettings() {
        return buildSettings;
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
        return new Program(
            realPath, idPath, exerptManager, tokens, buildSettings, structIds, functionIDCounter,
            scopeVarIDCounter, structsHere, parsedTypes_, externalDeclarations, scope
        );
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
                if (!func.IsMain()) continue;
                if (foundMain) {
                    throw new SyntaxErrorException(
                        "Only one main function can be defined", func
                    );
                }
                foundMain = true;
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
            ["structs_here"] = new JSONList(
                structsHere.Select(struct_ => new JSONString(struct_.GetID()))
            ),
            ["id_path"] = new JSONString(idPath),
            ["globals"] = new JSONList(Scope.GetVarsJSON(scope))
        };
        return obj;
    }
}
