using System;
using System.Collections.Generic;

public class FunctionHolder : Holder, IAnnotatable {
    List<IAnnotation> annotations;
    
    public FunctionHolder(List<IToken> tokens) : base(tokens) {
        annotations = new List<IAnnotation>();
    }

    public FunctionHolder(List<IToken> tokens, List<IAnnotation> annotations) : base(tokens) {
        this.annotations = annotations;
    }
    
    protected override TreeToken _Copy(List<IToken> tokens) {
        return (TreeToken)new FunctionHolder(tokens, annotations);
    }

    public RawFuncSignature GetRawSignature() {
        if (Count < 2) return null;
        IToken token = this[0];
        if (!(token is RawFuncSignature)) return null;
        return (RawFuncSignature)token;
    }

    public FuncSignature GetSignature() {
        if (Count < 2) return null;
        IToken token = this[0];
        if (!(token is FuncSignature)) return null;
        return (FuncSignature)token;
    }

    public void SetSignature(IToken signature) {
        if (Count < 2) {
            throw new InvalidOperationException(
                "FunctionHolder does not have signature already set"
            );
        }
        this[0] = signature;
    }

    public List<IAnnotation> GetAnnotations() {
        return annotations;
    }

    public AnnotationRecipients RecipientType() {
        return AnnotationRecipients.FUNCTION;
    }
    
    public void ApplyAnnotation(IAnnotation annotation) {
        annotations.Add(annotation);
    }
}
