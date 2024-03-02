using System;

public class ActionExpectation : Expectation {
    public ActionExpectation(Action action) {
        Then(action);
    }
    
    public override bool IsOptional() {return true;}
    public override bool IsEmpty() {return true;}
}
