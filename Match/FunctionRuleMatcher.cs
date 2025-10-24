namespace Epsilon;
public class FunctionRuleMatcher(PatternExtractor<List<IToken>> extractor) : IMatcher {
    readonly PatternExtractor<List<IToken>> extractor = extractor;

    public Match Match(IParentToken tokens) {
        int start = -1;
        int end = -1;
        List<IToken> replaced = [];
        extractor.SetCallback((replaced_, start_, end_) => {
            replaced = replaced_;
            start = start_;
            end = end_;
        });
        List<IToken> arguments = extractor.Extract(tokens);
        if (arguments == null) return null;
        return new Match(start, end, [
            new RawFunctionCall(extractor.GetSegments(), arguments)
        ], replaced);
    }
}
