namespace Epsilon;
public class TextPatternSegment(string text) : IPatternSegment {
    readonly string text = text;

    public string GetText() {
        return text;
    }

    public bool Matches(IToken token) {
        return token is TextToken
            && ((TextToken)token).GetText() == text;
    }

    public bool Equals(IPatternSegment obj) {
        TextPatternSegment other = obj as TextPatternSegment;
        if (other == null) return false;
        return text == other.GetText();
    }
}
