using System;

public class PresentExpectation : Expectation {
    public override bool IsOptional() {return true;}
    public override bool IsEmpty() {return true;}
}
