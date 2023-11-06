using System;
using System.Linq;
using System.Collections.Generic;

public class Instantiation : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    Type_ type_;
    List<IValueToken> values;
    
    public int Count {
        get {
            return values.Count;
        }
    }
    
    public IToken this[int i] {
        get {
            return values[i];
        }
        set {
            values[i] = ((IValueToken)value);
        }
    }
    
    public Instantiation(Type_ type_, List<IValueToken> values) {
        this.type_ = type_;
        this.values = values;
    }
    
    public Instantiation(Type_Token type_token, ValueList list) {
        this.type_ = type_token.GetValue();
        this.values = list.GetValues().Select(
            (ValueListItem token) => (IValueToken)token[0]
        ).ToList();
    }

    public Type_ GetType_() {
        return type_;
    }

    public List<IValueToken> GetValues() {
        return values;
    }

    public override string ToString() {
        return Utils.WrapName(
            Utils.WrapName(
                GetType().Name,
                type_.ToString(),
                "<", ">"
            ), 
            String.Join(
                ", ", values.ConvertAll<string>(
                    obj => obj.ToString()
                )
            )
        );
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
        );
    }
}
