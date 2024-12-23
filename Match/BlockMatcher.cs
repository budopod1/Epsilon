namespace Epsilon;
public class BlockMatcher : IMatcher {
    readonly IPatternSegment prior;
    readonly IPatternSegment start;
    readonly IPatternSegment end;
    readonly Type holder;

    public BlockMatcher(IPatternSegment start, IPatternSegment end, Type holder) {
        this.start = start;
        this.end = end;
        this.holder = holder;
    }

    public BlockMatcher(IPatternSegment prior, IPatternSegment start, IPatternSegment end, Type holder) {
        this.prior = prior;
        this.start = start;
        this.end = end;
        this.holder = holder;
    }

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            int indent = 0;
            bool any = false;
            List<IToken> replaced = [];
            for (int j = i; j < tokens.Count; j++) {
                IToken token = tokens[j];
                IToken priorToken = j > 0 ? tokens[j-1] : null;
                replaced.Add(token);

                bool priorMatches = prior == null || (
                    priorToken != null && prior.Matches(priorToken));
                if (start.Matches(token) && priorMatches) {
                    indent++;
                } else if (end.Matches(token)) {
                    if (!any) {
                        break;
                    }
                    indent--;
                }

                if (indent == 0) {
                    if (!any) break;
                    List<IToken> replacement = [];
                    List<IToken> replace = new(replaced);
                    replace.RemoveAt(0);
                    replace.RemoveAt(replace.Count-1);
                    IToken holderToken = (IToken)Activator.CreateInstance(holder, [replace]);
                    replacement.Add(holderToken);
                    return new Match(i, j, replacement, replaced);
                }
                any = true;
            }
            any = false;
        }
        return null;
    }
}
