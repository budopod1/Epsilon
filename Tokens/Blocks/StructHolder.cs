namespace Epsilon;
public class StructHolder : Holder, IAnnotatable {
    readonly List<IAnnotation> annotations;

    public StructHolder(List<IToken> tokens) : base(tokens) {
        annotations = [];
    }

    public StructHolder(List<IToken> tokens, List<IAnnotation> annotations) : base(tokens) {
        this.annotations = annotations;
    }

    protected override TreeToken _Copy(List<IToken> tokens) {
        return new StructHolder(tokens, annotations);
    }

    public List<IAnnotation> GetAnnotations() {
        return annotations;
    }

    public AnnotationRecipients RecipientType() {
        return AnnotationRecipients.STRUCT;
    }

    public void ApplyAnnotation(IAnnotation annotation) {
        annotations.Add(annotation);
    }
}
