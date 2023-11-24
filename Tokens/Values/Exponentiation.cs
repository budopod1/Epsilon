using System;

public class Exponentiation : BinaryOperation<IValueToken, IValueToken>, IValueToken {
    public Exponentiation(IValueToken o1, IValueToken o2) : base(o1, o2) {}

    public Type_ GetType_() {
        Type_ base_ = o1.GetType_();
        Type_ expo = o2.GetType_();
        if (expo.GetBaseType_().GetName() == "W") {
            return base_;
        }
        return Type_.CommonSpecific(base_, expo, "Q");
    }

    public override int Serialize(SerializationContext context) {
        string mode = "pow";
        double? exponentValue = null;
        IValueToken value = o2;
        while (value is Group) {
            value = ((Group)value).Sub();
        }
        ConstantValue expo = value as ConstantValue;
        if (expo != null) {
            IConstant iconst = expo.GetValue();
            INumberConstant inumconst = iconst as INumberConstant;
            if (inumconst != null) {
                double dv = inumconst.GetDoubleValue();
                exponentValue = dv;
                if (Math.Ceiling(dv) == dv) {
                    mode = "chain";
                } else if (Utils.ApproxEquals(dv, 1.0/2.0)) {
                    mode = "sqrt";
                } else if (Utils.ApproxEquals(dv, 1.0/3.0)) {
                    mode = "cbrt";
                }
            }
        }
        return context.AddInstruction(
            new SerializableInstruction(this, context).AddData("mode", new JSONString(mode))
                .AddData("exponent_value", new JSONDouble(exponentValue))
        );
    }
}
