using System;
using System.Linq;
using System.Collections.Generic;

public class TextsPatternSegment(List<string> texts) : IPatternSegment {
    readonly List<string> texts = texts;

    public List<string> GetTexts() {
        return texts;
    }

    public bool Matches(IToken token) {
        return token is TextToken
            && texts.Contains(((TextToken)token).GetText());
    }

    public bool Equals(IPatternSegment obj) {
        TextsPatternSegment other = obj as TextsPatternSegment;
        if (other == null) return false;
        return texts.SequenceEqual(other.GetTexts());
    }
}
