namespace Epsilon;
public class Match(int start, int end, List<IToken> replacement, List<IToken> matched) {
    readonly int start = start;
    readonly int end = end;
    readonly List<IToken> replacement = replacement;
    readonly List<IToken> matched = matched;

    void UpdateSpans(List<IToken> tokens) {
        CodeSpan span = TokenUtils.MergeSpans(tokens);
        foreach (IToken token in replacement) {
            token.span = span;
        }
    }

    public TreeToken Replace(TreeToken tokens) {
        List<IToken> result = [];
        List<IToken> formerTokens = [];
        for (int j = start; j <= end; j++) {
            formerTokens.Add(tokens[j]);
        }
        UpdateSpans(formerTokens);
        int i = 0;
        foreach (IToken token in tokens) {
            if (i > end || i < start) {
                result.Add(token);
            } else if (i == start) {
                result.AddRange(replacement);
            }
            i++;
        }
        return tokens.Copy(result);
    }

    public void SingleReplace(IParentToken tokens) {
        if (Length() != 1) {
            throw new InvalidOperationException(
                "SingleReplace can only be used on Matches with length 1"
            );
        }
        UpdateSpans([tokens[start]]);
        tokens[start] = replacement[0];
    }

    public List<IToken> GetMatched() {
        return matched;
    }

    public int Length() {
        return end - start + 1;
    }

    public int GetStart() {
        return start;
    }

    public int GetEnd() {
        return end;
    }

    public override string ToString() {
        string result = "(" + start.ToString();
        result += ", " + end.ToString() + "):";
        for (int i = 0; i < matched.Count; i++) {
            result += matched[i].ToString();
        }
        result += "|";
        for (int i = 0; i < replacement.Count; i++) {
            result += replacement[i].ToString();
        }
        return Utils.WrapName(GetType().Name, result);
    }
}
