using System;
using System.Reflection;
using System.Collections.Generic;

public class FunctionObjectifyerMatcher : IMatcher {
    Type newType;
    
    public FunctionObjectifyerMatcher(Type newType) {
        this.newType = newType;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            Token token = tokens[i];
            if (token is FunctionHolder) {
                FunctionHolder holder = ((FunctionHolder)token);
                FuncTemplate template = holder.GetTemplate();
                Token replacement = (Token)Activator.CreateInstance(
                    newType, new object[] {
                        template.GetValue(), template.GetArguments(),
                        holder.GetBlock()
                    }
                );
                return new Match(i, i, new List<Token> {replacement},
                                 new List<Token> {token});
            }
        }
        return null;
    }
}
