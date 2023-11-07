using System;
using System.Collections.Generic;

public class Program : TreeToken, IVerifier { 
    List<string> baseType_Names = null;
    
    public Program(List<IToken> tokens) : base(tokens) {}
    
    public Program(List<IToken> tokens, List<string> baseType_Names) : base(tokens) {
        this.baseType_Names = baseType_Names;
    }

    public List<string> GetBaseType_Names() {
        return baseType_Names;
    }

    public void UpdateParents() {
        TokenUtils.UpdateParents(this);
        parent = null;
    }

    public void SetBaseType_Names(List<string> baseType_Names) {
        this.baseType_Names = baseType_Names;
    }
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return new Program(tokens, baseType_Names);
    }

    public void Verify() {
        foreach (IToken token in this) {
            if (!(token is ITopLevel)) {
                throw new SyntaxErrorException(
                    "Invalid toplevel syntax", token
                );
            }
        }
    }

    public Struct GetStructFromType_(Type_ type_) {
        string name = type_.GetBaseType_().GetName();
        foreach (IToken token in this) {
            Struct struct_ = token as Struct;
            if (struct_ == null) continue;
            if (struct_.GetName() == name) {
                return struct_;
            } 
        }
        return null;
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        JSONList functions = new JSONList();
        JSONList structs = new JSONList();
        foreach (IToken token in this) {
            if (token is Function) {
                functions.Add(((Function)token).GetJSON());
            } else if (token is Struct) {
                structs.Add(((Struct)token).GetJSON());
            }
        }
        obj["functions"] = functions;
        obj["structs"] = structs;
        return obj;
    }
}
