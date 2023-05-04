using System;

public class TextToken : IToken {
    public string Text;
    
    public TextToken(string text) {
        this.Text = text;
    }

    public override string ToString() {
        return this.Text;
    }
}
