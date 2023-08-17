using System;

public class TextToken : Token {
    string text;
    
    public TextToken(string text) {
        this.text = text;
    }

    public string GetText() {
        return text;
    }

    public void SetText(string text) {
        this.text = text;
    }

    public override string ToString() {
        return this.text;
    }
}
