using CsJSONTools;

namespace Epsilon;
public class Exponentiation(IValueToken o1, IValueToken o2) : BinaryOperation<IValueToken, IValueToken>(o1, o2), IValueToken {
    public Type_ GetType_() {
        Type_ base_ = o1.GetType_();
        Type_ expo = o2.GetType_();
        if (expo.GetBaseType_().GetName() == "W") {
            return base_;
        }
        return Type_.CommonSpecificNonNull(this, base_, expo, "Q");
    }

    public override int UncachedSerialize(SerializationContext context) {
        IValueToken value = o2;
        bool negate = false;

        while (true) {
            if (value is Group group) {
                value = group.Sub();
            } else if (value is Negation negation) {
                value = negation.Sub();
                negate = !negate;
            } else {
                break;
            }
        }

        string mode = "pow";
        IJSONValue exponentValue = new JSONNull();
        if (value is ConstantValue expo) {
            IConstant iconst = expo.GetValue();
            if (iconst is IIntConstant intconst) {
                mode = "chain";
                exponentValue = new JSONInt(intconst.GetIntValue());
            } else if (iconst is INumberConstant numconst) {
                double dv = numconst.GetDoubleValue();
                if (negate) dv = -dv;
                exponentValue = new JSONDouble(dv);
                if (Utils.ApproxEquals(dv, 1.0 / 2.0)) {
                    mode = "sqrt";
                } else if (Utils.ApproxEquals(dv, 1.0 / 3.0)) {
                    mode = "cbrt";
                }
            }
        }

        return new SerializableInstruction(context, this) {
            ["mode"] = mode,
            ["exponent_value"] = exponentValue
        }.SetOperands([o1, o2]).Register();
    }
}
