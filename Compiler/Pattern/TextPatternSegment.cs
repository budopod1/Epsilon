using System;

public class TextPatternSegment : IPatternSegment {
    string text;

    public string GetText() {
        return text;
    }
    
    public TextPatternSegment(string text) {
        this.text = text;
    }

    public bool Matches(IToken token) {
        return (token is TextToken 
            && ((TextToken)token).GetText() == text);
    }

    public bool Equals(IPatternSegment obj) {
        TextPatternSegment other = obj as TextPatternSegment;
        if (other == null) return false;
        return text == other.GetText();
    }
}
