using System;
using System.Linq;
using System.Collections.Generic;

public abstract class RealFunctionDeclaration : FunctionDeclaration {
    public abstract Type_ GetReturnType_();
    public abstract string GetCallee();
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
}
