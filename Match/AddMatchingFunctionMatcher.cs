namespace Epsilon;
public class AddMatchingFunctionMatcher(FunctionDeclaration func) : IMatcher {
    readonly FunctionDeclaration func = func;
    readonly List<IPatternSegment> segments = func.GetPattern().GetSegments();

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            if (token is not RawFunctionCall call) continue;
            List<IPatternSegment> csegments = call.GetSegments();
            if (csegments.SequenceEqual(segments)) {
                call.AddMatchingFunction(func);
            }
        }
        return null;
    }
}
