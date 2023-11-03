using System;
using System.Linq;
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
        yield return token;
    }

    public static IEnumerable<T> TraverseFind<T>(IParentToken token) {
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
        if (token is T) yield return (T)token;
    }

    public static CodeSpan MergeSpans(List<IToken> tokens) {
        return CodeSpan.Merge(tokens.Select(token => token.span));
    }

    public static CodeSpan MergeSpans(IToken a, IToken b) {
        return MergeSpans(new List<IToken> {a, b});
    }

    public static bool FullMatch(List<IPatternSegment> segs, 
                                 List<IToken> tokens) {
        if (segs.Count != tokens.Count) return false;
        for (int i = 0; i < segs.Count; i++) {
            if (!segs[i].Matches(tokens[i])) return false;
        }
        return true;
    }
}
