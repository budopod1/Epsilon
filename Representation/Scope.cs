using System;
using System.Collections.Generic;

public class Scope {
    CodeBlock block;
    Dictionary<int, ScopeVar> variables = new Dictionary<int, ScopeVar>();
    static int id = 0;

    public Scope(CodeBlock block) {
        this.block = block;
    }

    bool containsVarLocal(int id) {
        return variables.ContainsKey(id);
    }

    public bool ContainsVar(string name) {
        foreach (ScopeVar svar in variables.Values) {
            if (svar.GetName() == name) return true; 
        }
        Scope parent = GetParentScope();
        if (parent == null) return false;
        return parent.ContainsVar(name);
    }
    
    public ScopeVar GetVarByID(int id) {
        if (!containsVarLocal(id)) return GetParentScope()?.GetVarByID(id);
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
        variables[id] = new ScopeVar(name, type_);
        return id++;
    }

    public static Scope GetEnclosing(IToken token) {
        CodeBlock block = TokenUtils.GetParentOfType<CodeBlock>(token);
        return block?.GetScope();
    }

    public IEnumerable<IJSONValue> GetVarsJSON() {
        foreach (KeyValuePair<int, ScopeVar> pair in variables) {
            yield return pair.Value.GetJSON(pair.Key);
        }
    }

    public Scope GetParentScope() {
        CodeBlock parent = TokenUtils.GetParentOfType<CodeBlock>(block);
        return parent?.GetScope();
    }

    public Dictionary<int, ScopeVar> GetAllVars() {
        return variables;
    }

    public void CopyFrom(Scope other) {
        variables = new Dictionary<int, ScopeVar>(other.GetAllVars());
    }
}
