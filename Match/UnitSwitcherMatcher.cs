using System;
using System.Reflection;
using System.Collections.Generic;

public class UnitSwitcherMatcher<T> : IMatcher where T : IEquatable<T> {
    Type matchType;
    List<T> matchValues;
    Type replaceType;
    
    public UnitSwitcherMatcher(Type matchType, List<T> matchValues, Type replaceType) {
        this.matchType = matchType;
        this.matchValues = matchValues;
        this.replaceType = replaceType;
    }
    
    public Match Match(TreeToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            
            if (!Utils.IsInstance(token, matchType)) continue;
            
            Unit<T> unit = ((Unit<T>)token);
            T value = unit.GetValue();
            foreach (T matchValue in matchValues) {
                if (matchValue.Equals(value)) {
                    List<IToken> replacement = new List<IToken> {
                        (Unit<T>)Activator.CreateInstance(
                            replaceType, new object[] {value}
                        )
                    };
                    List<IToken> replaced = new List<IToken> {token};
                    return new Match(i, i, replacement, replaced);
                }
            }
        }
        return null;
    }
}
