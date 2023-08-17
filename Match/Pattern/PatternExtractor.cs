using System;
using System.Collections.Generic;

abstract public class PatternExtractor<T> {
    protected List<IPatternSegment> segments;
    protected IPatternProcessor<T> processor;
    
    public T Extract(ParentToken tokens) {
        int maxStart = tokens.Count - segments.Count;
        for (int i = 0; i < maxStart; i++) {
            bool matches = true;
            int j;
            List<Token> tokenList = new List<Token>();
            for (j = 0; j < segments.Count; j++) {
                IPatternSegment segment = segments[j];
                Token token = tokens[i+j];
                tokenList.Add(token);
                if (!segment.Matches(token)) {
                    matches = false;
                    break;
                }
            }
            if (matches) {
                return processor.Process(tokenList, i, i+j-1);
            }
        }
        return default(T);
    }
}
