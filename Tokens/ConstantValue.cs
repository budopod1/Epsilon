using System;

public class ConstantValue : Unit<int>, IValueToken {
    public ConstantValue(int constant) : base(constant) {}

    public Type_ GetType_() {
        return Type_.Unknown(); // temp
    }
}
