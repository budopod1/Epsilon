using System;
using System.Collections.Generic;

public class Scope {
    Dictionary<int, ScopeVar> variables = new Dictionary<int, ScopeVar>();
    static int id = 0;

    public bool ContainsVar(int id) {
        return variables.ContainsKey(id);
    }

    public bool ContainsVar(string name) {
        foreach (ScopeVar svar in variables.Values) {
            if (svar.GetName() == name) return true; 
        }
        return false;
    }
    
    public ScopeVar GetVarByID(int id) {
        if (!ContainsVar(id)) return null;
        return variables[id];
    }
    
    public ScopeVar GetVarByName(string name) {
        foreach (ScopeVar svar in variables.Values) {
            if (svar.GetName() == name) return svar; 
        }
        return null;
    }

    public int? GetIDByName(string name) {
        foreach (KeyValuePair<int, ScopeVar> pair in variables) {
            if (pair.Value.GetName() == name) return pair.Key;
        }
        return null;
    }

    public int AddVar(string name, Type_ type_) {
        variables[id] = new ScopeVar(name, type_);
        return id++;
    }

    public static Scope GetEnclosing(IToken token) {
        Function func = TokenUtils.GetParentOfType<Function>(token);
        if (func == null) return null;
        return func.GetScope();
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        foreach (KeyValuePair<int, ScopeVar> pair in variables) {
            obj[pair.Key.ToString()] = pair.Value.GetJSON();
        }
        return obj;
    }

    public override string ToString() {
        return GetJSON().ToJSON();
    }
}
