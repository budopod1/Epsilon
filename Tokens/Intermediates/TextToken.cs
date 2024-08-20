using System;

public class TextToken(string text) : IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    string text = text;

    public string GetText() {
        return text;
    }

    public void SetText(string text) {
        this.text = text;
    }

    public override string ToString() {
        return text;
    }

    public void Verify() {
        throw new SyntaxErrorException(
            $"Unmatched char '{text}'", this
        );
    }
}
