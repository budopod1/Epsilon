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
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count-1; i++) {
            IToken a = tokens[i];
            IToken b = tokens[i+1];
            if (Utils.IsInstance(a, nameType) && Utils.IsInstance(b, blockType)) {
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
