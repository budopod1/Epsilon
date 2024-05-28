using System;
using System.Collections.Generic;

public interface IScope {
    bool ContainsVar(string name);
    ScopeVar GetVarByID(int id);
    ScopeVar GetVarByName(string name);
    int? GetIDByName(string name);
    int AddVar(string name, Type_ type_);
    Dictionary<int, ScopeVar> GetAllVars();
    void CopyFrom(IScope other);
}
