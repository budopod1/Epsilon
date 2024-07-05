using System;
using System.Collections.Generic;

public class RawAnnotation : TreeToken {
    string type;
    
    public RawAnnotation(string type, List<IToken> tokens) : base(tokens) {
        this.type = type;
    }

    public IAnnotation ToAnnotation() {
        switch (type) {
            case "id":
                return IDAnnotation.FromTokens(this, GetTokens());
            case "super":
                return SuperAnnotation.FromTokens(this, GetTokens());
            case "extends":
                return ExtendsAnnotation.FromTokens(this, GetTokens());
            default:
                throw new SyntaxErrorException($"Invalid annotation type {type}", this);
        }
    }

    public string AnnotationTypeName() {
        return type;
    }

    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new RawAnnotation(type, tokens);
    }
}
