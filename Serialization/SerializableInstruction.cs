public class SerializableInstruction {
    readonly SerializationContext ctx;
    readonly JSONObject obj = [];

    public object this[string key] {
        set => obj[key] = ctx.ObjToJSON(value);
    }

    public SerializableInstruction(SerializationContext ctx, ISerializableToken token) {
        this.ctx = ctx;
        obj["type"] = new JSONString(Utils.CammelToSnake(token.GetType().Name));
        SetOperands([]);
        if (token is IValueToken valueToken) {
            obj["type_"] = valueToken.GetType_().GetJSON();
        }
    }

    public SerializableInstruction SetOperands(IEnumerable<ISerializableToken> operands) {
        obj["operands"] = new JSONList(operands.Select(
            operand => new JSONInt(ctx.Serialize(operand))
        ));
        return this;
    }

    public int Register() {
        return ctx.AddInstructionJSON(obj);
    }
}
