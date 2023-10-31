using System;
using System.Collections.Generic;

public class Switch : IFlowControl {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    IValueToken value;
    List<SwitchArm> arms;
    CodeBlock default_;

    public int Count {
        get { return 1 + arms.Count + (default_==null?0:1); }
    }
    
    public IToken this[int i] {
        get {
            if (i == 0) return value;
            if (i == arms.Count+1) return default_;
            return arms[i-1];
        }
        set {
            if (i == 0) {
                this.value = (IValueToken)value;
            } else if (i == arms.Count+1) {
                default_ = (CodeBlock)value;
            } else {
                arms[i-1] = (SwitchArm)value;
            }
        }
    }

    public Switch(IValueToken value, List<SwitchArm> arms, CodeBlock default_=null) {
        this.value = value;
        this.arms = arms;
        this.default_ = default_;
    }

    public Switch(IValueToken value, IToken[] rest) {
        this.value = value;
        arms = new List<SwitchArm>();
        int max = rest.Length;
        if (max % 2 == 1) {
            max--;
            default_ = (CodeBlock)rest[rest.Length-1];
        }
        for (int i = 0; i < max; i+=2) {
            SwitchArm arm = new SwitchArm(
                (IValueToken)rest[i],
                (CodeBlock)rest[i+1]
            );
            arm.span = TokenUtils.MergeSpans(rest[i], rest[i+1]);
            arms.Add(arm);
        }
    }

    public IValueToken GetValue() {
        return value;
    }

    public List<SwitchArm> GetArms() {
        return arms;
    }

    public CodeBlock GetDefault() {
        return default_;
    }

    public override string ToString() {
        string result = value.ToString() + ": ";
        foreach (SwitchArm arm in arms) {
            result += arm.ToString() + ", ";
        }
        if (default_ != null) {
            result += "Default: " + default_.ToString();
        }
        return Utils.WrapName("Switch", result);
    }
}
