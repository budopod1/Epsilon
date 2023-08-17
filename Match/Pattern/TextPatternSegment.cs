using System;

public class TextPatternSegment : IPatternSegment {
    string text;
    
    public TextPatternSegment(string text) {
        this.text = text;
    }

    public bool Matches(Token token) {
        return (token is TextToken 
            && ((TextToken)token).GetText() == text);
    }
}
