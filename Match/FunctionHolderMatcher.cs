using System;
using System.Reflection;
using System.Collections.Generic;

public class FunctionHolderMatcher : IMatcher {
    Type templateType;
    Type blockType;
    Type holderType;
    
    public FunctionHolderMatcher(Type template, Type block, Type holder) {
        templateType = template;
        blockType = block;
        holderType = holder;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            IToken a = tokens[i];
            IToken b = tokens[i+1];
            if (Utils.IsInstance(a, templateType) && Utils.IsInstance(b, blockType)) {
                List<IToken> replaced = new List<IToken> {a, b};
                IToken holder = (IToken)Activator.CreateInstance(
                    holderType, new object[] {replaced}
                );
                return new Match(i, i+1, new List<IToken> {holder}, replaced);
            }
        }
        return null;
    }
}
