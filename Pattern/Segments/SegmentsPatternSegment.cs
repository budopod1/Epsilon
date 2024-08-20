using System;
using System.Linq;
using System.Collections.Generic;

public class SegmentsPatternSegment(List<IPatternSegment> segments) : IPatternSegment {
    readonly List<IPatternSegment> segments = segments;

    public List<IPatternSegment> GetSegments() {
        return segments;
    }

    public bool Matches(IToken token) {
        IParentToken parent = token as IParentToken;
        if (parent == null) return false;
        if (segments.Count != parent.Count) return false;
        for (int i = 0; i < parent.Count; i++) {
            if (!segments[i].Matches(parent[i])) return false;
        }
        return true;
    }

    public bool Equals(IPatternSegment obj) {
        SegmentsPatternSegment other = obj as SegmentsPatternSegment;
        if (other == null) return false;
        return segments.SequenceEqual(other.GetSegments());
    }
}
