public class UnitSwitcherMatcher<TOld, TNew>(Type matchType, Func<TOld, TNew> replacer, Type replaceType) : IMatcher {
    readonly Type matchType = matchType;
    readonly Func<TOld, TNew> replacer = replacer;
    readonly Type replaceType = replaceType;

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];

            if (!Utils.IsInstance(token, matchType)) continue;

            Unit<TOld> unit = (Unit<TOld>)token;
            TOld value = unit.GetValue();
            TNew replacement = replacer(value);
            if (replacement != null) {
                List<IToken> replacementTokens = [
                    (Unit<TNew>)Activator.CreateInstance(replaceType, [replacement])
                ];
                List<IToken> replaced = [token];
                return new Match(i, i, replacementTokens, replaced);
            }
        }
        return null;
    }
}
