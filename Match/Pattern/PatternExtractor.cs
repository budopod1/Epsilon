using System;
using System.Collections.Generic;

abstract public class PatternExtractor<T> {
    protected List<IPatternSegment> segments;
    protected PatternProcessor<T> processor;
    
    public T Extract(TreeToken tokens) {
        int maxStart = tokens.Count - segments.Count;
        for (int i = 0; i < maxStart; i++) {
            bool matches = true;
            int j;
            List<IToken> tokenList = new List<IToken>();
            for (j = 0; j < segments.Count; j++) {
                IPatternSegment segment = segments[j];
                IToken token = tokens[i+j];
                tokenList.Add(token);
                if (!segment.Matches(token)) {
                    matches = false;
                    break;
                }
            }
            if (matches) {
                return processor.Process(tokenList, i, i+j);
            }
        }
        return default(T);
    }
}
