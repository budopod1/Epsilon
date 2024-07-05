using System;
using System.Collections.Generic;

public class SuperAnnotation : IAnnotation {
    CodeSpan span;

    public SuperAnnotation(CodeSpan span) {
        this.span = span;
    }
    
    public static SuperAnnotation FromTokens(IToken base_, List<IToken> arguments) {
        if (arguments.Count > 0) {
            throw new SyntaxErrorException(
                "Expected no arguments for super annotation", base_
            );
        }
        return new SuperAnnotation(TokenUtils.MergeSpans(arguments));
    }

    public CodeSpan GetSpan() {
        return span;
    }

    public AnnotationRecipients GetRecipients() {
        return AnnotationRecipients.STRUCT;
    }
}
