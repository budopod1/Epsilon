using System;

public interface IAnnotatable : IToken {
    AnnotationRecipients RecipientType();
    void ApplyAnnotation(IAnnotation annotation);
}
