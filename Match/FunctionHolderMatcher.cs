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
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            Token a = tokens[i];
            Token b = tokens[i+1];
            if (Utils.IsInstance(a, templateType) && Utils.IsInstance(b, blockType)) {
                List<Token> replaced = new List<Token> {a, b};
                Token holder = (Token)Activator.CreateInstance(
                    holderType, new object[] {replaced}
                );
                return new Match(i, i+1, new List<Token> {holder}, replaced);
            }
        }
        return null;
    }
}
