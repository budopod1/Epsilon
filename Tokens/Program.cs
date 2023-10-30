using System;
using System.Collections.Generic;

public class Program : TreeToken, IVerifier { 
    Constants constants;
    List<string> baseType_Names = null;
    
    public Program(List<IToken> tokens,
                    Constants constants) : base(tokens) {
        this.constants = constants;
    }
    
    public Program(List<IToken> tokens, Constants constants,
                   List<string> baseType_Names) : base(tokens) {
        this.constants = constants;
        this.baseType_Names = baseType_Names;
    }

    public Constants GetConstants() {
        return constants;
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
        return new Program(tokens, constants, baseType_Names);
    }

    public void Verify() {
        foreach (IToken token in this) {
            if (!(token is ITopLevel)) {
                throw new SyntaxErrorException(
                    "Invalid toplevel syntax"
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
}
