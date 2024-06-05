using System;
using System.Linq;
using System.Collections.Generic;

public abstract class RealFunctionDeclaration : FunctionDeclaration, IEquatable<RealFunctionDeclaration> {
    public abstract Type_ GetReturnType_();
    public abstract string GetCallee();
    public abstract string GetSourcePath();
    public abstract bool TakesOwnership();
    public abstract bool ResultInParams();
    
    public JSONObject GetJSON() {
        JSONObject obj = new JSONObject();
        obj["id"] = new JSONString(GetID());
        obj["callee"] = new JSONString(GetCallee());
        obj["arguments"] = new JSONList(GetArguments().Select(
            argument => argument.GetJSON()
        ));
        obj["return_type_"] = GetReturnType_().GetJSON();
        obj["takes_ownership"] = new JSONBool(TakesOwnership());
        obj["result_in_params"] = new JSONBool(ResultInParams());
        return obj;
    }

    public bool Equals(RealFunctionDeclaration other) {
        if (!GetReturnType_().Equals(other.GetReturnType_())) return false;
        if (GetCallee() != other.GetCallee()) return false;
        if (TakesOwnership() != other.TakesOwnership()) return false;
        if (ResultInParams() != other.ResultInParams()) return false;
        if (!Utils.ListEqual(GetArguments(), other.GetArguments())) return false;
        if (GetSource() != other.GetSource()) return false;
        return GetPattern().Equals(other.GetPattern());
    }

    public sealed override Type_ GetReturnType_(List<IValueToken> tokens) {
        return GetReturnType_();
    }

    public bool IsMain() {
        return GetCallee() == "main";
    }
    
    public override bool DoesReturnVoid() {
        return GetReturnType_().GetBaseType_().IsVoid();
    }
}
