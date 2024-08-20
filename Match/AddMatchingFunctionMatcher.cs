using System;
using System.Linq;
using System.Collections.Generic;

public class AddMatchingFunctionMatcher(FunctionDeclaration func) : IMatcher {
    readonly FunctionDeclaration func = func;
    readonly List<IPatternSegment> segments = func.GetPattern().GetSegments();

    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            RawFunctionCall call = token as RawFunctionCall;
            if (call == null) continue;
            List<IPatternSegment> csegments = call.GetSegments();
            if (csegments.SequenceEqual(segments)) {
                call.AddMatchingFunction(func);
            }
        }
        return null;
    }
}
