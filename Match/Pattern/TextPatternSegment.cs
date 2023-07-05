/*
using System;

public class TextPatternSegment : IPatternSegment {
    string text;
    
    public TokenPatternSegment(string text) {
        this.text = text;
    }

    public bool Matches(IToken token) {
        return (token is TextToken 
            && ((TextToken)token).Text == text);
    }

    public void Reset() {}
}
*/