using System;
using System.Collections.Generic;

public class ExtendsAnnotation(CodeSpan span, string extendee) : IAnnotation {
    readonly CodeSpan span = span;
    readonly string extendee = extendee;

    public static ExtendsAnnotation FromTokens(IToken base_, List<IToken> arguments) {
        if (arguments.Count == 0) {
            throw new SyntaxErrorException(
                $"Expected struct name", base_
            );
        }
        if (arguments.Count >= 2) {
            throw new SyntaxErrorException(
                $"Too many arguments for extends annotation", arguments[1]
            );
        }
        Name name = arguments[0] as Name;
        if (name == null) {
            throw new SyntaxErrorException(
                $"Expected name", name
            );
        }
        return new ExtendsAnnotation(TokenUtils.MergeSpans(arguments), name.GetValue());
    }

    public CodeSpan GetSpan() {
        return span;
    }

    public AnnotationRecipients GetRecipients() {
        return AnnotationRecipients.STRUCT;
    }

    public string GetExtendee() {
        return extendee;
    }
}
