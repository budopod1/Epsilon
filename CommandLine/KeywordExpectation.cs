using System;

public class KeywordExpectation : Expectation {
    string keyword;
    bool isOptional;
    
    public KeywordExpectation(string keyword, bool isOptional) {
        this.keyword = keyword;
        this.isOptional = isOptional;
    }

    public override bool Matches(string word) {return keyword == word;}
    public override bool IsOptional() {return isOptional;}
    public override bool IsEmpty() {return false;}
    protected override string _GetHelp() {return keyword;}
}
