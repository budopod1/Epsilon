using System;
using System.Reflection;
using System.Collections.Generic;

public class Type_Matcher : IMatcher {
    Type baseType;
    Type genericsType;
    Type replaceType;
    ListTokenParser<Type_> listParser;
    
    public Type_Matcher(Type baseType, Type genericsType, Type replaceType, 
                        ListTokenParser<Type_> listParser) {
        this.baseType = baseType;
        this.genericsType = genericsType;
        this.replaceType = replaceType;
        this.listParser = listParser;
    }
    
    public Match Match(ParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            Token name = tokens[i];
            
            if (Utils.IsInstance(name, baseType)) {
                Unit<BaseType_> nameUnit = ((Unit<BaseType_>)name);
                Type_ type_;
                List<Token> replacement;
                List<Token> replaced;
                if (i + 1 < tokens.Count) {
                    Token next = tokens[i + 1];
                    if (Utils.IsInstance(next, genericsType)) {
                        ParentToken generics = ((ParentToken)next);
                        List<Type_> genericTypes_ = listParser.Parse(generics);
                        if (genericTypes_ == null) continue;
                        type_ = new Type_(nameUnit.GetValue(), genericTypes_);
                        replacement = new List<Token> {
                            (Token)Activator.CreateInstance(
                                replaceType, new object[] {type_}
                            )
                        };
                        replaced = new List<Token> {
                            name, generics
                        };
                        return new Match(i, i+1, replacement, replaced);
                    }
                }
                type_ = new Type_(nameUnit.GetValue());
                replacement = new List<Token> {
                    (Token)Activator.CreateInstance(
                        replaceType, new object[] {type_}
                    )
                };
                replaced = new List<Token> {name};
                return new Match(i, i, replacement, replaced);
            }
        }
        return null;
    }
}
