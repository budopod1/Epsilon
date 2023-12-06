using System;
using System.Reflection;
using System.Collections.Generic;

public class Type_Matcher : IMatcher {
    ListTokenParser<Type_> listParser = new ListTokenParser<Type_>(
        new TextPatternSegment(","), typeof(Type_Token), 
        (IToken generic) => ((Type_Token)generic).GetValue()
    );
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken name = tokens[i];
            
            if (name is UserBaseType_Token) {
                UserBaseType_Token nameUnit = ((UserBaseType_Token)name);
                Type_ type_ = Type_.FromUserBaseType_(nameUnit.GetValue());
                int j = i;
                List<IToken> replaced = new List<IToken> {name};
                if (i + 1 < tokens.Count) {
                    IToken next = tokens[i + 1];
                    if (next is Generics) {
                        Generics generics = ((Generics)next);
                        List<Type_> genericTypes_ = listParser.Parse(generics);
                        if (genericTypes_ == null) continue;
                        type_ = Type_.FromUserBaseType_(nameUnit.GetValue(), genericTypes_);
                        replaced.Add(generics);
                        j++;
                    }
                }
                List<IToken> replacement = new List<IToken> {new Type_Token(type_)};
                return new Match(i, j, replacement, replaced);
            }
        }
        return null;
    }
}
