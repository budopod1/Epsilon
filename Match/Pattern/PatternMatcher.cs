///*
using System;
using System.Collections.Generic;

public class PatternMatcher : PatternExtractor<Match>, IMatcher {
    public PatternMatcher(List<IPatternSegment> segments, 
                          PatternProcessor<List<IToken>> subprocessor) {
        this.segments = segments;
        this.processor = new MatcherPatternProcessor(subprocessor);
    }

    public Match Match(TreeToken tokens) {
        return this.Extract(tokens);
    }
}

/*
using System;
using System.Collections.Generic;

public class PatternMatcher : IMatcher {
    List<IPatternSegment> segments;
    PatternProcessor<List<IToken>> processor;

    public PatternMatcher(List<IPatternSegment> segments, 
                          PatternProcessor<List<IToken>> processor) {
        this.segments = segments;
        this.processor = processor;
    }
    
    public Match Match(TreeToken tokens) {
        int maxStart = tokens.Count - segments.Count;
        for (int i = 0; i < maxStart; i++) {
            bool matches = true;
            int j;
            List<IToken> replaced = new List<IToken>();
            for (j = 0; j < segments.Count; j++) {
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
                    i, i+j, processor.Process((replaced)), replaced
                );
            }
        }
        return null;
    }
}
*/
