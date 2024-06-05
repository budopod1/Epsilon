using System;
using System.Collections.Generic;

public abstract class FunctionDeclaration : IComparable<FunctionDeclaration> {
    public abstract PatternExtractor<List<IToken>> GetPattern();
    public abstract List<FunctionArgument> GetArguments();
    public abstract string GetID();
    public abstract FunctionSource GetSource();
    public abstract bool DoesReturnVoid();
    public abstract Type_ GetReturnType_(List<IValueToken> tokens);

    public int CompareTo(FunctionDeclaration other) {
        FunctionDeclaration a = this;
        FunctionDeclaration b = other;
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
        for (int i = 0; i < aac; i++) {
            Type_ argaType_ = aa[i].GetType_();
            Type_ argbType_ = ba[i].GetType_();
            bool aGtB = argaType_.IsGreaterThan(argbType_);
            bool bGtA = argbType_.IsGreaterThan(argaType_);
            if (aGtB && bGtA) continue;
            if (aGtB) return 1;
            if (bGtA) return -1;
        }
        FunctionSource aSrc = a.GetSource();
        FunctionSource bSrc = b.GetSource();
        return bSrc.CompareTo(aSrc);
    }
}
