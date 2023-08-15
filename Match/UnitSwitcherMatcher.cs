using System;
using System.Reflection;
using System.Collections.Generic;

public class UnitSwitcherMatcher<TOld, TNew> : IMatcher {
    Type matchType;
    Func<TOld, TNew> replacer;
    Type replaceType;
    
    public UnitSwitcherMatcher(Type matchType, Func<TOld, TNew> replacer, Type replaceType) {
        this.matchType = matchType;
        this.replacer = replacer;
        this.replaceType = replaceType;
    }
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken token = tokens[i];
            
            if (!Utils.IsInstance(token, matchType)) continue;
            
            Unit<TOld> unit = ((Unit<TOld>)token);
            TOld value = unit.GetValue();
            TNew replacement = replacer(value);
            if (replacement != null) {
                List<IToken> replacementTokens = new List<IToken> {
                    (Unit<TNew>)Activator.CreateInstance(
                        replaceType, new object[] {replacement}
                    )
                };
                List<IToken> replaced = new List<IToken> {token};
                return new Match(i, i, replacementTokens, replaced);
            }
        }
        return null;
    }
}
