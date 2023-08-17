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
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count-2; i++) {
            IToken a = tokens[i];
            IToken b = tokens[i+1];
            IToken c = tokens[i+2];
            if (Utils.IsInstance(a, type_Type) && Utils.IsInstance(b, declareType)
                && Utils.IsInstance(c, varType)) {
                List<IToken> replaced = new List<IToken> {a, b, c};
                IToken result = (IToken)Activator.CreateInstance(
                    varDeclareType, new object[] {
                        new List<IToken> {a, c}
                    }
                );
                return new Match(i, i+2, new List<IToken> {result}, replaced);
            }
        }
        return null;
    }
}
