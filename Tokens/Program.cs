using System;
using System.Collections.Generic;

public class Program : TreeToken { 
    Constants constants;
    List<string> baseType_Names = null;
    
    public Program(List<Token> tokens,
                    Constants constants) : base(tokens) {
        this.constants = constants;
    }
    
    public Program(List<Token> tokens, Constants constants,
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

    public void SetBaseType_Names(List<string> baseType_Names) {
        this.baseType_Names = baseType_Names;
    }

    public void UpdateParents() {
        UpdateParents(this);
    }

    public static void UpdateParents(ParentToken token) {
        for (int i = 0; i < token.Count; i++) {
            Token sub = token[i];
            sub.parent = token;
            if (sub is ParentToken) {
                UpdateParents(sub);
            }
        }
    }
    
    public override TreeToken Copy(List<Token> tokens) {
        return (TreeToken)new Program(tokens, constants,
                                      baseType_Names);
    }

    public static Program GetProgram(Token token) {
        Token current = token;
        int i = 0;
        while (!(current is Program)) {
            current = current.parent;
            if (++i > 1000) {
                throw new ArgumentException("Cannot find parent within 1000 iterations: there may be a circular refrence");
            }
            if (current == null) return null;
        }
        return (Program)current;
    }
}
