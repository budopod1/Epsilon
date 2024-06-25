using System;
using System.Collections.Generic;

public class IDAnnotation : IAnnotation {
    string id;

    public IDAnnotation(string id) {
        this.id = id;
    }

    public static IDAnnotation FromTokens(IToken base_, List<IToken> arguments) {
        if (arguments.Count == 0) {
            throw new SyntaxErrorException(
                $"Expected id", base_
            );
        }
        if (arguments.Count >= 2) {
            throw new SyntaxErrorException(
                $"Too many arguments for ID annotation", arguments[1]
            );
        }
        Name name = arguments[0] as Name;
        if (name == null) {
            throw new SyntaxErrorException(
                $"Expected name", name
            );
        }
        return new IDAnnotation(name.GetValue());
    }

    public AnnotationRecipients GetRecipients() {
        return AnnotationRecipients.STRUCT | AnnotationRecipients.FUNCTION;
    }

    public string GetID() {
        return id;
    }
}
