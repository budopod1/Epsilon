using System;
using System.Collections.Generic;

public class Scope : IScope {
    IDCounter scopeVarIDCounter;
    Dictionary<int, ScopeVar> variables = new Dictionary<int, ScopeVar>();

    public Scope(IDCounter scopeVarIDCounter) {
        this.scopeVarIDCounter = scopeVarIDCounter;
    }

    T WithParentScope<T>(IHasScope hs, Func<IScope, IHasScope, T> action) {
        if (hs == null) return default(T);
        IHasScope parenths = TokenUtils.GetParentOfType<IHasScope>(hs);
        return action(parenths?.GetScope(), parenths);
    }

    bool ContainsVarLocal(int id) {
        return variables.ContainsKey(id);
    }

    public bool ContainsVar(IHasScope hs, string name) {
        foreach (ScopeVar svar in variables.Values) {
            if (svar.GetName() == name) return true; 
        }
        return WithParentScope(hs, (parent, parenths) => {
            if (parent == null) return false;
            return parent.ContainsVar(parenths, name);
        });
    }
    
    public ScopeVar GetVarByID(IHasScope hs, int id) {
        if (ContainsVarLocal(id)) return variables[id];
        return WithParentScope(hs, (parent, parenths)
            => parent?.GetVarByID(parenths, id));
    }
    
    public ScopeVar GetVarByName(IHasScope hs, string name) {
        foreach (ScopeVar svar in variables.Values) {
            if (svar.GetName() == name) return svar; 
        }
        return WithParentScope(hs, (parent, parenths)
            => parent?.GetVarByName(parenths, name));
    }

    public int? GetIDByName(IHasScope hs, string name) {
        foreach (KeyValuePair<int, ScopeVar> pair in variables) {
            if (pair.Value.GetName() == name) return pair.Key;
        }
        return WithParentScope(hs, (parent, parenths)
            => parent?.GetIDByName(parenths, name));
    }

    public int AddVar(string name, Type_ type_) {
        int id = scopeVarIDCounter.GetID();
        variables[id] = new ScopeVar(name, type_);
        return id;
    }

    public Dictionary<int, ScopeVar> GetAllVars() {
        return variables;
    }

    public static IScope GetEnclosing(IToken token) {
        CodeBlock block = TokenUtils.GetParentOfType<CodeBlock>(token);
        return block?.GetScope();
    }

    public static bool ContainsVar(IToken token, string name) {
        IHasScope hs = TokenUtils.GetParentOfType<IHasScope>(token);
        return hs.GetScope().ContainsVar(hs, name);
    }

    public static ScopeVar GetVarByID(IToken token, int id) {
        IHasScope hs = TokenUtils.GetParentOfType<IHasScope>(token);
        return hs.GetScope().GetVarByID(hs, id);
    }

    public static int? GetIDByName(IToken token, string name) {
        IHasScope hs = TokenUtils.GetParentOfType<IHasScope>(token);
        return hs.GetScope().GetIDByName(hs, name);
    }

    public static IEnumerable<IJSONValue> GetVarsJSON(IScope scope) {
        foreach (KeyValuePair<int, ScopeVar> pair in scope.GetAllVars()) {
            yield return pair.Value.GetJSON(pair.Key);
        }
    }
}
