using System;
using System.Reflection;
using System.Collections.Generic;

public class Type_Matcher : IMatcher {
    Func<List<IToken>, Func<Type_>, List<IToken>> type_Wrapper;

    public Type_Matcher(Func<List<IToken>, Func<Type_>, List<IToken>> type_Wrapper) {
        this.type_Wrapper = type_Wrapper;
    }

    ListTokenParser<Type_> listParser = new ListTokenParser<Type_>(
        new TextPatternSegment(","), typeof(Type_Token),
        (IToken generic) => ((Type_Token)generic).GetValue()
    );

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];

            UserBaseType_Token baseType_Token = token as UserBaseType_Token;
            if (baseType_Token == null) continue;
            UserBaseType_ baseType_ = baseType_Token.GetValue();

            int j = i;

            List<IToken> replaced = new List<IToken> {token};

            List<IToken> replacement = null;
            if (i + 1 < tokens.Count) {
                IToken next = tokens[i + 1];
                Generics generics = next as Generics;
                if (generics != null) {
                    List<Type_> genericTypes_ = listParser.Parse(generics);
                    if (genericTypes_ == null) continue;
                    replaced.Add(generics);
                    replacement = type_Wrapper(replaced,
                        () => baseType_.ToType_(genericTypes_));
                    j++;
                }
            }

            replacement = replacement ?? type_Wrapper(replaced, () => baseType_.ToType_());

            return new Match(i, j, replacement, replaced);
        }
        return null;
    }
}
