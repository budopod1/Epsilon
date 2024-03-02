using System;

public class InputExpectation : Expectation {
    string help;

    public InputExpectation(string help) {
        this.help = help;
    }
    
    public override bool Matches(string word) {return true;}
    public override bool IsOptional() {return false;}
    public override bool IsEmpty() {return false;}
    protected override string _GetHelp() {return $"<{help}>";}
}
