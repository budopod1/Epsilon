using System;
using System.Collections.Generic;

public class Switch : IFlowControl, IVerifier {
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
            Group group = ((Group)rest[i]);
            ConstantValue constant = group.Sub() as ConstantValue;
            if (constant == null) {
                throw new SyntaxErrorException(
                    "Switch arm values can only be constants", group
                );
            }
            SwitchArm arm = new SwitchArm(
                constant, (CodeBlock)rest[i+1]
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

    public int Serialize(SerializationContext context) {
        JSONList armsJSON = new JSONList();
        foreach (SwitchArm arm in arms) {
            CodeBlock armBlock = arm.GetBlock();
            SerializationContext sub = context.AddSubContext(armBlock.GetScope());
            sub.Serialize(armBlock);
            JSONObject armObj = new JSONObject();
            armObj["block"] = new JSONInt(sub.GetIndex());
            armObj["target"] = arm.GetTarget().GetValue().GetJSON();
            armsJSON.Add(armObj);
        }
        IJSONValue defaultJSON = new JSONNull();
        if (default_ != null) {
            SerializationContext sub = context.AddSubContext(default_.GetScope());
            sub.Serialize(default_);
            defaultJSON = new JSONInt(sub.GetIndex());
        }
        return context.AddInstruction(
            new SerializableInstruction(
                "switch", new List<int> {value.Serialize(context)}
            ).AddData("arms", armsJSON).AddData("default", defaultJSON)
             .AddData("value_type_", value.GetType_().GetJSON())
        );
    }

    public void Verify() {
        Type_ valueType_ = value.GetType_();
        BaseType_ baseValueType_ = valueType_.GetBaseType_();
        bool isInt = baseValueType_.IsInt();
        bool isFloat = baseValueType_.IsFloat();
        if (!isInt && !isFloat) {
            throw new SyntaxErrorException(
                $"Cannot switch on type {valueType_}", value
            );
        }
        foreach (SwitchArm arm in arms) {
            BaseType_ targetBaseType_ = arm.GetTarget().GetType_().GetBaseType_();
            if (isInt) {
                if (!targetBaseType_.IsInt()) {
                    throw new SyntaxErrorException(
                        $"Switches on an integer type must have integer targets", arm.GetTarget()
                    );
                }
            }
            if (isFloat) {
                if (!targetBaseType_.IsFloat()) {
                    throw new SyntaxErrorException(
                        $"Switches on an float type must have float targets", arm.GetTarget()
                    );
                }
            }
        }
    }
}
