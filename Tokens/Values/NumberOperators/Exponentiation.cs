using System;

public class Exponentiation(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken {
    public Type_ GetType_() {
        Type_ base_ = o1.GetType_();
        Type_ expo = o2.GetType_();
        if (expo.GetBaseType_().GetName() == "W") {
            return base_;
        }
        return Type_.CommonSpecificNonNull(this, base_, expo, "Q");
    }

    public override int Serialize(SerializationContext context) {
        string mode = "pow";
        double? exponentValue = null;
        IValueToken value = o2;
        bool negate = false;
        while (true) {
            if (value is Group) {
                value = ((Group)value).Sub();
            } else if (value is Negation) {
                value = ((Negation)value).Sub();
                negate = !negate;
            } else {
                break;
            }
        }
        ConstantValue expo = value as ConstantValue;
        if (expo != null) {
            IConstant iconst = expo.GetValue();
            INumberConstant inumconst = iconst as INumberConstant;
            if (inumconst != null) {
                double dv = inumconst.GetDoubleValue();
                if (negate) dv = -dv;
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
            new SerializableInstruction(this, context)
                .AddData("mode", new JSONString(mode))
                .AddData("exponent_value", JSONDouble.OrNull(exponentValue))
        );
    }
}
