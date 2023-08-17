using System;
using System.Collections.Generic;

public class Program : TreeToken { 
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
        UpdateParents(this);
    }

    public static void UpdateParents(IParentToken token) {
        for (int i = 0; i < token.Count; i++) {
            IToken sub = token[i];
            sub.parent = token;
            if (sub is IParentToken)
                UpdateParents((IParentToken)token);
        }
    }

    public void SetBaseType_Names(List<string> baseType_Names) {
        this.baseType_Names = baseType_Names;
    }
    
    public override TreeToken Copy(List<IToken> tokens) {
        return (TreeToken)new Program(tokens, constants,
                                      baseType_Names);
    }
}
