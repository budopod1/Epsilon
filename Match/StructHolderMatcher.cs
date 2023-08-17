using System;
using System.Reflection;
using System.Collections.Generic;

public class StructHolderMatcher : IMatcher {
    Type nameType;
    Type blockType;
    Type holderType;
    
    public StructHolderMatcher(Type name, Type block, Type holder) {
        nameType = name;
        blockType = block;
        holderType = holder;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            Token a = tokens[i];
            Token b = tokens[i+1];
            if (Utils.IsInstance(a, nameType) && Utils.IsInstance(b, blockType)) {
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
