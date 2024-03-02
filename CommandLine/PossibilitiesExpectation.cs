using System;
using System.Linq;
using System.Collections.Generic;

public class PossibilitiesExpectation : Expectation {
    List<string> options;

    public PossibilitiesExpectation(params string[] options) {
        this.options = options.ToList();
    }

    public override bool Matches(string word) {return options.Contains(word);}
    public override bool IsOptional() {return false;}
    public override bool IsEmpty() {return false;}
    protected override string _GetHelp() {
        return "<"+String.Join(" | ", options)+">";
    }
}
