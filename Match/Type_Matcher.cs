namespace Epsilon;
public class Type_Matcher(Func<List<IToken>, Func<Type_>, List<IToken>> type_Wrapper) : IMatcher {
    readonly Func<List<IToken>, Func<Type_>, List<IToken>> type_Wrapper = type_Wrapper;
    readonly ListTokenParser<Type_> listParser = new(
        new TextPatternSegment(","), typeof(Type_Token),
        generic => ((Type_Token)generic).GetValue()
    );

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];

            if (token is not UserBaseType_Token baseType_Token) continue;
            UserBaseType_ baseType_ = baseType_Token.GetValue();

            int j = i;

            List<IToken> replaced = [token];

            List<IToken> replacement = null;
            if (i + 1 < tokens.Count) {
                IToken next = tokens[i + 1];
                if (next is Generics generics) {
                    List<Type_> genericTypes_ = listParser.Parse(generics);
                    if (genericTypes_ == null) continue;
                    replaced.Add(generics);
                    replacement = type_Wrapper(replaced,
                        () => baseType_.ToType_(genericTypes_));
                    j++;
                }
            }

            replacement ??= type_Wrapper(replaced, baseType_.ToType_);

            return new Match(i, j, replacement, replaced);
        }
        return null;
    }
}
