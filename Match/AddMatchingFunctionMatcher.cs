using System;
using System.Linq;
using System.Collections.Generic;

public class AddMatchingFunctionMatcher : IMatcher {
    Function func;
    List<IPatternSegment> segments;
    
    public AddMatchingFunctionMatcher(Function func) {
        this.func = func;
        segments = func.GetPattern().GetSegments();
    }
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            RawFunctionCall call = token as RawFunctionCall;
            if (call == null) continue;
            List<IPatternSegment> csegments = call.GetSegments();
            if (Utils.ListEqual<IPatternSegment>(csegments, segments)) {
                call.AddMatchingFunction(func);
            }
        }
        return null;
    }
}
