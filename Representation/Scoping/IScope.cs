namespace Epsilon;
public interface IScope {
    bool ContainsVar(IHasScope hs, string name);
    ScopeVar GetVarByID(IHasScope hs, int id);
    ScopeVar GetVarByName(IHasScope hs, string name);
    int? GetIDByName(IHasScope hs, string name);
    int AddVar(string name, Type_ type_);
    Dictionary<int, ScopeVar> GetAllVars();
}
