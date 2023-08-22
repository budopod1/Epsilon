using System;

public class ConstantValue : Unit<int>, IValueToken {
    public ConstantValue(int constant) : base(constant) {}

    public Type_ GetType_() {
        Program program = TokenUtils.GetParentOfType<Program>(this);
        Constants constants = program.GetConstants();
        IConstant constant = constants.GetConstant(GetValue());
        return constant.GetType_();
    }
}
