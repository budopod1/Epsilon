using System;
using System.Reflection;
using System.Collections.Generic;

public class FunctionConverterMatcher : IMatcher {
    Type newType;
    
    public FunctionConverterMatcher(Type newType) {
        this.newType = newType;
    }
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (token is FunctionHolder) { // TEMP (see TODO.txt)
                FunctionHolder raw = ((FunctionHolder)token);
                FuncTemplate template = raw.GetTemplate();
                Block block = raw.GetBlock();
                IToken replacement = (IToken)Activator.CreateInstance(
                    newType, new object[] {
                        template.GetValue(), template.GetArguments(), block
                    }
                );
                return new Match(i, i, new List<IToken> {replacement},
                                 new List<IToken> {token});
            }
        }
        return null;
    }
}
