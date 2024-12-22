namespace Epsilon;
public static class TokenUtils {
    public static void UpdateParents(IParentToken token) {
        for (int i = 0; i < token.Count; i++) {
            IToken sub = token[i];
            sub.parent = token;
            if (sub is IParentToken token1)
                UpdateParents(token1);
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
                return default;
            }
        } while (current is not T);
        return (T)current;
    }

    public static IEnumerable<IToken> Traverse(IParentToken token, TraverseConfig config) {
        int i = config.Invert ? token.Count-1 : 0;
        if (config.YieldFirst) yield return token;
        while (0 <= i && i < token.Count) {
            IToken sub = token[i];
            i += config.Invert ? -1 : 1;
            if (config.AvoidTokens(sub)) continue;
            if (sub is IParentToken parent) {
                foreach (IToken subsub in Traverse(parent, config)) {
                    yield return subsub;
                }
            } else {
                yield return sub;
            }
        }
        if (!config.YieldFirst) yield return token;
    }

    public static IEnumerable<IToken> Traverse(IParentToken token) {
        foreach (IToken sub in Traverse(token, new TraverseConfig())) {
            yield return sub;
        }
    }

    public static IEnumerable<T> TraverseFind<T>(IParentToken token, TraverseConfig config) {
        foreach (IToken sub in Traverse(token, config)) {
            if (sub is T t) yield return t;
        }
    }

    public static IEnumerable<T> TraverseFind<T>(IParentToken token) {
        foreach (T sub in TraverseFind<T>(token, new TraverseConfig())) {
            yield return sub;
        }
    }

    public static CodeSpan MergeSpans(IEnumerable<IToken> tokens) {
        return CodeSpan.Merge(tokens.Select(token => token.span));
    }

    public static CodeSpan MergeSpans(IToken a, IToken b) {
        return MergeSpans([a, b]);
    }

    public static bool FullMatch(List<IPatternSegment> segs, List<IToken> tokens) {
        if (segs.Count != tokens.Count) return false;
        for (int i = 0; i < segs.Count; i++) {
            if (!segs[i].Matches(tokens[i])) return false;
        }
        return true;
    }

    public static string GenerateErrorFrame(IToken token) {
        Function function = GetParentOfType<Function>(token);
        Program program = GetParentOfType<Program>(function);
        FileExerptManager exerptManager = program.GetExerptManager();
        int spanStart = token.span.GetStart();
        int spanEnd = token.span.GetEnd();
        int startLine = exerptManager.GetLineIdxFromPos(spanStart);
        int endLine = exerptManager.GetLineIdxFromPos(spanEnd);
        string exerpt = exerptManager.GetLineFromIdx(startLine).Trim();
        string annotatedExerpt = "\n    "+exerpt;
        // We should only show the annotation if there is something other
        // than the annotated section in the exerpt
        int irrelevantContentThreshold = 4;
        if (startLine == endLine
            && spanEnd - spanStart + irrelevantContentThreshold < exerpt.Length) {
            string annotation = exerptManager.AnnotateLine(spanStart, spanEnd, "┗", "━", "┛");
            annotatedExerpt += "\n    "+annotation;
        }
        string locationInfo = $"  File \"{program.GetRealPath()}\", line {startLine+1}";
        return locationInfo + annotatedExerpt;
    }
}
