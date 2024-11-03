namespace Epsilon;
public class GroupConverterMatcher(Type source, Type dest) : IMatcher {
    readonly Type source = source;
    readonly Type dest = dest;

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (Utils.IsInstance(token, source)) {
                TreeToken tree = (TreeToken)token;
                if (tree.Count == 1) {
                    IToken inner = tree[0];
                    if (inner is IValueToken token1) {
                        IToken result = (IToken)Activator.CreateInstance(
                            dest, [token1]
                        );
                        return new Match(
                            i, i, [result], [token]
                        );
                    }
                }
            }
        }
        return null;
    }
}
