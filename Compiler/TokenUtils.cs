using System;
using System.Linq;
using System.Collections.Generic;

public static class TokenUtils {
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
        do {
            current = current.parent;
            if (current == null || ++i >= 1000) {
                return null;
            }
        } while (!cond(current));
        return current;
    }

    public static T GetParentOfType<T>(IToken token) {
        IToken current = token;
        int i = 0;
        do {
            current = current.parent;
            if (current == null || ++i >= 1000) {
                return default(T);
            }
        } while (!(current is T));
        return (T)current;
    }

    public static IEnumerable<IToken> Traverse(IParentToken token, TraverseConfig config) {
        int i = config.Invert ? token.Count-1 : 0;
        while (0 <= i && i < token.Count) {
            IToken sub = token[i];
            if (sub is IParentToken) {
                foreach (IToken subsub in Traverse((IParentToken)sub)) {
                    yield return subsub;
                }
            } else {
                yield return sub;
            }
            i += config.Invert ? -1 : 1;
        }
        yield return token;
    }

    public static IEnumerable<IToken> Traverse(IParentToken token) {
        foreach (IToken sub in Traverse(token, new TraverseConfig())) {
            yield return sub;
        }
    }

    public static IEnumerable<T> TraverseFind<T>(IParentToken token, TraverseConfig config) {
        foreach (IToken sub in Traverse(token, config)) {
            if (sub is T) yield return (T)sub;
        }
    }
    
    public static IEnumerable<T> TraverseFind<T>(IParentToken token) {
        foreach (T sub in TraverseFind<T>(token, new TraverseConfig())) {
            yield return sub;
        }
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
