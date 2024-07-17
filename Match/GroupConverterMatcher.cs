using System;
using System.Reflection;
using System.Collections.Generic;

public class GroupConverterMatcher : IMatcher {
    Type source;
    Type dest;

    public GroupConverterMatcher(Type source, Type dest) {
        this.source = source;
        this.dest = dest;
    }

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (Utils.IsInstance(token, source)) {
                TreeToken tree = ((TreeToken)token);
                if (tree.Count == 1) {
                    IToken inner = tree[0];
                    if (inner is IValueToken) {
                        IToken result = (IToken)Activator.CreateInstance(
                            dest, new object[] {(IValueToken)inner}
                        );
                        return new Match(
                            i, i, new List<IToken> {result}, new List<IToken> {token}
                        );
                    }
                }
            }
        }
        return null;
    }
}
