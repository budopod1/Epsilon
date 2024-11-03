namespace Epsilon;
public class RawGivenValue : TreeToken {
    public RawGivenValue(List<IToken> tokens) : base(tokens) {
        span = TokenUtils.MergeSpans(tokens);
    }

    protected override TreeToken _Copy(List<IToken> tokens) {
        return new RawGivenValue(tokens);
    }
}
