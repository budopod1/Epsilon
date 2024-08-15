using System;
using System.Collections.Generic;

public class FunctionShapeComparer : IComparer<FunctionDeclaration> {
    FunctionShapeComparer() {}

    public static readonly FunctionShapeComparer Singleton = new FunctionShapeComparer();

    public int Compare(FunctionDeclaration a, FunctionDeclaration b) {
        int apsc = a.GetPattern().GetSegments().Count;
        int bpsc = b.GetPattern().GetSegments().Count;
        if (apsc < bpsc) return 1;
        if (apsc > bpsc) return -1;
        List<FunctionArgument> aa = a.GetArguments();
        List<FunctionArgument> ba = b.GetArguments();
        int aac = aa.Count;
        int bac = ba.Count;
        if (aac < bac) return 1;
        if (aac > bac) return -1;
        FunctionSource aSrc = a.GetSource();
        FunctionSource bSrc = b.GetSource();
        return bSrc.CompareTo(aSrc);
    }
}
