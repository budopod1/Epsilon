using System;
using System.Collections.Generic;

public class StringLiteral : IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    string str;

    public StringLiteral(string str) {
        this.str = str;
    }

    public Type_ GetType_() {
        return Type_.String();
    }

    public virtual int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this)
                .AddData("string", new JSONString(str))
        );
    }
}
