using System;
using System.Collections.Generic;

public class CombinedMatchersMatcher : IMatcher {
    List<IMatcher> matchers;
    
    public CombinedMatchersMatcher(List<IMatcher> matchers) {
        this.matchers = matchers;
    }
    
    public Match Match(IParentToken tokens) {
        int minStart = tokens.Count+1;
        Match currentMatch = null;
        foreach (IMatcher matcher in matchers) {
            Match match = matcher.Match(tokens);
            if (match == null) continue;
            int start = match.GetStart();
            if (start < minStart) {
                minStart = start;
                currentMatch = match;
            }
        }
        return currentMatch;
    }
}
