using System;
using System.Collections.Generic;

public class TokenUtils {
    public static void UpdateParents(IParentToken token) {
        for (int i = 0; i < token.Count; i++) {
            IToken sub = token[i];
            sub.parent = token;
            if (sub is IParentToken)
                UpdateParents((IParentToken)sub);
        }
    }

    public static IToken GetParentWithCond(IToken token, Func<IToken, bool> cond) {
        IToken current = token;
        int i = 0;
        while (!cond(current)) {
            current = current.parent;
            if (current == null || ++i >= 1000) {
                return null;
            }
        }
        return current;
    }

    public static T GetParentOfType<T>(IToken token) {
        IToken current = token;
        int i = 0;
        while (!(current is T)) {
            current = current.parent;
            if (current == null || ++i >= 1000) {
                return default(T);
            }
        }
        return (T)current;
    }

    public static IEnumerable<IToken> Traverse(IParentToken token) {
        yield return token;
        for (int i = 0; i < token.Count; i++) {
            IToken sub = token[i];
            if (sub is IParentToken) {
                foreach (IToken subsub in Traverse((IParentToken)sub)) {
                    yield return subsub;
                }
            } else {
                yield return sub;
            }
        }
    }

    public static IEnumerable<T> TraverseFind<T>(IParentToken token) {
        if (token is T) yield return (T)token;
        for (int i = 0; i < token.Count; i++) {
            IToken sub = token[i];
            if (sub is IParentToken) {
                foreach (T subsub in TraverseFind<T>((IParentToken)sub)) {
                    yield return subsub;
                }
            } else if (sub is T) {
                yield return (T)sub;
            }
        }
    }
}
