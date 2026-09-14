namespace Epsilon;
public class ValueAnnotation(CodeSpan span) : IAnnotation {
    readonly CodeSpan span = span;

    public static ValueAnnotation FromTokens(IToken base_, List<IToken> arguments) {
        if (arguments.Count > 0) {
            throw new SyntaxErrorException(
                "Expected no arguments for value annotation", base_
            );
        }
        return new ValueAnnotation(TokenUtils.MergeSpans(arguments));
    }

    public CodeSpan GetSpan() {
        return span;
    }

    public AnnotationRecipients GetRecipients() {
        return AnnotationRecipients.STRUCT;
    }
}
