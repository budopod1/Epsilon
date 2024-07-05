using System;

public interface IAnnotation {
    CodeSpan GetSpan();
    AnnotationRecipients GetRecipients();
}
