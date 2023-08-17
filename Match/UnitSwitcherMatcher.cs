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
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            Token token = tokens[i];
            
            if (!Utils.IsInstance(token, matchType)) continue;
            
            Unit<TOld> unit = ((Unit<TOld>)token);
            TOld value = unit.GetValue();
            TNew replacement = replacer(value);
            if (replacement != null) {
                List<Token> replacementTokens = new List<Token> {
                    (Unit<TNew>)Activator.CreateInstance(
                        replaceType, new object[] {replacement}
                    )
                };
                List<Token> replaced = new List<Token> {token};
                return new Match(i, i, replacementTokens, replaced);
            }
        }
        return null;
    }
}
