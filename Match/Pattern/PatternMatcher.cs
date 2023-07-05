/*
using System;
using System.Collections.Generic;

public class PatternMatcher : IMatcher {
    List<IPatternSegment> segments;
    Func<List<IToken>, IToken> result;

    public PatternMatcher(List<IPatternSegment> segments, Func<List<IToken>, IToken> result) {
        this.segments = segments;
        this.result = result;
    }
    
    public Match Match(IToken tokens_) {
        TreeToken tokens = (TreeToken)tokens_;
        int maxStart = tokens.Count - segments.Count;
        for (int i = 0; i < maxStart; i++) {
            foreach (IPatternSegment segment in this.segments) {
                segment.Reset();
            }
            bool matches = true;
            int j;
            List<IToken> replaced = new List<IToken>();
            for (j = 0; j < segments.Count j++) {
                IPatternSegment segment = segments[j];
                IToken token = tokens[i+j];
                replaced.Add(token);
                if (!segment.Matches(token)) {
                    matches = false;
                    break;
                }
            }
            if (matches) {
                return new Match(
                    i, i+j, this.result(replaced), replaced
                );
            }
        }
        return null;
    }
}
*/
