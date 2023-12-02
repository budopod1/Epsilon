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

    // TODO: make IFunctionDeclaration into abstract class, move OrderFunctions to FunctionDeclaration class 
    public static int OrderFunctions(IFunctionDeclaration a, IFunctionDeclaration b) {
        int apsc = a.GetPattern().GetSegments().Count;
        int bpsc = b.GetPattern().GetSegments().Count;
        if (apsc < bpsc) return -1;
        if (apsc > bpsc) return 1;
        List<FunctionArgument> aa = a.GetArguments();
        List<FunctionArgument> ba = b.GetArguments();
        int aac = aa.Count;
        int bac = ba.Count;
        if (aac < bac) return -1;
        if (aac > bac) return 1;
        for (int i = 0; i < aac; i++) {
            Type_ argaType_ = aa[i].GetType_();
            Type_ argbType_ = ba[i].GetType_();
            bool aToB = argaType_.IsConvertibleTo(argbType_);
            bool bToA = argbType_.IsConvertibleTo(argaType_);
            if (aToB && bToA) continue;
            if (aToB) return -1;
            if (bToA) return 1;
        }
        return a.GetID() - b.GetID();
    }
}
