/*
using System;
using System.Reflection;
using System.Collections.Generic;

public class FunctionObjectifyerMatcher : IMatcher {
    Type newType;
    
    public FunctionObjectifyerMatcher(Type newType) {
        this.newType = newType;
    }
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (token is FunctionHolder) {
                FunctionHolder holder = ((FunctionHolder)token);
                FuncSignature sig = holder.GetSignature();
                FuncTemplate template = sig.GetTemplate();
                IToken replacement = (IToken)Activator.CreateInstance(
                    newType, new object[] {
                        template.GetValue(), template.GetArguments(),
                        holder.GetBlock(), sig.GetReturnType_()
                    }
                );
                return new Match(i, i, new List<IToken> {replacement},
                                 new List<IToken> {token});
            }
        }
        return null;
    }
}
*/
