using System;
using System.Collections.Generic;

public class Scope : IScope {
    IDCounter scopeVarIDCounter;
    CodeBlock block; // FIXME: Scope should not store CodeBlock
    Dictionary<int, ScopeVar> variables = new Dictionary<int, ScopeVar>();

    public Scope(IDCounter scopeVarIDCounter, CodeBlock block) {
        this.scopeVarIDCounter = scopeVarIDCounter;
        this.block = block;
    }

    bool ContainsVarLocal(int id) {
        return variables.ContainsKey(id);
    }

    public bool ContainsVar(string name) {
        foreach (ScopeVar svar in variables.Values) {
            if (svar.GetName() == name) return true; 
        }
        IScope parent = GetParentScope();
        if (parent == null) return false;
        return parent.ContainsVar(name);
    }
    
    public ScopeVar GetVarByID(int id) {
        if (!ContainsVarLocal(id)) return GetParentScope()?.GetVarByID(id);
        return variables[id];
    }
    
    public ScopeVar GetVarByName(string name) {
        foreach (ScopeVar svar in variables.Values) {
            if (svar.GetName() == name) return svar; 
        }
        return GetParentScope()?.GetVarByName(name);
    }

    public int? GetIDByName(string name) {
        foreach (KeyValuePair<int, ScopeVar> pair in variables) {
            if (pair.Value.GetName() == name) return pair.Key;
        }
        return GetParentScope()?.GetIDByName(name);
    }

    public int AddVar(string name, Type_ type_) {
        int id = scopeVarIDCounter.GetID();
        variables[id] = new ScopeVar(name, type_);
        return id;
    }

    public static IScope GetEnclosing(IToken token) {
        CodeBlock block = TokenUtils.GetParentOfType<CodeBlock>(token);
        return block?.GetScope();
    }

    public static IEnumerable<IJSONValue> GetVarsJSON(IScope scope) {
        foreach (KeyValuePair<int, ScopeVar> pair in scope.GetAllVars()) {
            yield return pair.Value.GetJSON(pair.Key);
        }
    }

    IScope GetParentScope() {
        if (block == null) return null;
        IHasScope parent = TokenUtils.GetParentOfType<IHasScope>(block);
        return parent?.GetScope();
    }

    public Dictionary<int, ScopeVar> GetAllVars() {
        return variables;
    }

    public void CopyFrom(IScope other) {
        variables = new Dictionary<int, ScopeVar>(other.GetAllVars());
    }
}
