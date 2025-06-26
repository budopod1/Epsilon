namespace Epsilon;
public class ConcreteAnnotation(CodeSpan span) : IAnnotation {
    readonly CodeSpan span = span;

    public static ConcreteAnnotation FromTokens(IToken base_, List<IToken> arguments) {
        if (arguments.Count > 0) {
            throw new SyntaxErrorException(
                "Expected no arguments for super annotation", base_
            );
        }
        return new ConcreteAnnotation(TokenUtils.MergeSpans(arguments));
    }

    public CodeSpan GetSpan() {
        return span;
    }

    public AnnotationRecipients GetRecipients() {
        return AnnotationRecipients.STRUCT;
    }
}
