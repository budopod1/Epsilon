using System;

public class TextToken : IToken {
    public string text;
    
    public TextToken(string text) {
        this.text = text;
    }

    public override string ToString() {
        return this.text;
    }
}
