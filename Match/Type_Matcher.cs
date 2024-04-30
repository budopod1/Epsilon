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
            IToken token = tokens[i];
            
            UserBaseType_Token baseType_Token = token as UserBaseType_Token;
            if (baseType_Token == null) continue;
            
            int j = i;
            List<IToken> replaced = new List<IToken> {token};
            
            try {
                Type_ type_ = null;
                if (i + 1 < tokens.Count) {
                    IToken next = tokens[i + 1];
                    Generics generics = next as Generics;
                    if (generics != null) {
                        List<Type_> genericTypes_ = listParser.Parse(generics);
                        if (genericTypes_ == null) continue;
                        replaced.Add(generics);
                        type_ = Type_.FromUserBaseType_(
                            baseType_Token.GetValue(), genericTypes_
                        );
                        j++;
                    }
                }

                if (type_ == null) {
                    type_ = Type_.FromUserBaseType_(baseType_Token.GetValue());
                }

                List<IToken> replacement = new List<IToken> {new Type_Token(type_)};
                return new Match(i, j, replacement, replaced);
            } catch (IllegalType_Exception e) {
                throw new SyntaxErrorException(
                    e.Message, TokenUtils.MergeSpans(replaced)
                );
            }
        }
        return null;
    }
}
