using System;
using System.Collections.Generic;

public abstract class Expectation {
    public string Matched = null;
    public bool IsPresent = false;
    List<Action> thens = new List<Action>();
    
    public virtual bool Matches(string word) {return false;}
    public abstract bool IsOptional();
    public abstract bool IsEmpty();
    protected virtual string _GetHelp() {return "";}
    public string GetHelp() {return _GetHelp();}

    public Expectation Then(Action action) {
        thens.Add(action);
        return this;
    }

    public void RunThens() {
        foreach (Action action in thens) {
            action();
        }
    }
}
