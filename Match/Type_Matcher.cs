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
    
    public Match Match(IParentToken tokens) {
        for (int i = 0; i < tokens.Count; i++) {
            IToken name = tokens[i];
            
            if (Utils.IsInstance(name, baseType)) {
                try {
                    Unit<UserBaseType_> nameUnit = ((Unit<UserBaseType_>)name);
                    Type_ type_;
                    List<IToken> replacement;
                    List<IToken> replaced;
                    if (i + 1 < tokens.Count) {
                        IToken next = tokens[i + 1];
                        if (Utils.IsInstance(next, genericsType)) {
                            IParentToken generics = ((IParentToken)next);
                            List<Type_> genericTypes_ = listParser.Parse(generics);
                            if (genericTypes_ == null) continue;
                            type_ = Type_.FromUserBaseType_(nameUnit.GetValue(), genericTypes_);
                            replacement = new List<IToken> {
                                (IToken)Activator.CreateInstance(
                                    replaceType, new object[] {type_}
                                )
                            };
                            replaced = new List<IToken> {
                                name, generics
                            };
                            return new Match(i, i+1, replacement, replaced);
                        }
                    }
                    type_ = Type_.FromUserBaseType_(nameUnit.GetValue());
                    replacement = new List<IToken> {
                        (IToken)Activator.CreateInstance(
                            replaceType, new object[] {type_}
                        )
                    };
                    replaced = new List<IToken> {name};
                    return new Match(i, i, replacement, replaced);
                } catch (BaseType_BitsException e) {
                    throw new SyntaxErrorException(e.Message, name);
                }
            }
        }
        return null;
    }
}
