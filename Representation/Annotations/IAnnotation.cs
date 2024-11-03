namespace Epsilon;
public interface IAnnotation {
    CodeSpan GetSpan();
    AnnotationRecipients GetRecipients();
}
