using System;
using System.Reflection;
using System.Collections.Generic;

public class VarDeclareMatcher : IMatcher {
    Type varType;
    Type declareType;
    Type type_Type;
    Type varDeclareType;
    
    public VarDeclareMatcher(Type varType, Type declareType, Type type_Type,
                             Type varDeclareType) {
        this.varType = varType;
        this.declareType = declareType;
        this.type_Type = type_Type;
        this.varDeclareType = varDeclareType;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count-2; i++) {
            Token a = tokens[i];
            Token b = tokens[i+1];
            Token c = tokens[i+2];
            if (Utils.IsInstance(a, type_Type) && Utils.IsInstance(b, declareType)
                && Utils.IsInstance(c, varType)) {
                List<Token> replaced = new List<Token> {a, b, c};
                Token result = (Token)Activator.CreateInstance(
                    varDeclareType, new object[] {
                        new List<Token> {a, c}
                    }
                );
                return new Match(i, i+2, new List<Token> {result}, replaced);
            }
        }
        return null;
    }
}
