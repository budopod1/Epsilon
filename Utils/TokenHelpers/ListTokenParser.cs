namespace Epsilon;
public class ListTokenParser<T>(IPatternSegment seperator, Type item, Func<IToken, T> parser) {
    enum ParseState {
        EXPECTITEM,
        EXPECTSEPARATOR
    }

    readonly IPatternSegment seperator = seperator;
    readonly Type item = item;
    readonly Func<IToken, T> parser = parser;

    public List<T> Parse(IParentToken tree) {
        List<T> list = [];
        ParseState state = ParseState.EXPECTITEM;
        for (int i = 0; i < tree.Count; i++) {
            IToken token = tree[i];
            switch (state) {
            case ParseState.EXPECTITEM:
                if (Utils.IsInstance(token, item)) {
                    list.Add(parser(token));
                    state = ParseState.EXPECTSEPARATOR;
                } else {
                    return null;
                }
                break;
            case ParseState.EXPECTSEPARATOR:
                if (seperator.Matches(token)) {
                    state = ParseState.EXPECTITEM;
                } else {
                    return null;
                }
                break;
            }
        }
        return list;
    }
}
