using System;
using System.Linq;
using System.Collections.Generic;

public class Instantiation : IParentToken, IValueToken, IVerifier {
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
        this.values = list.GetValues().Where(token=>token.Count>0).Select(
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

    public void Verify() {
        Program program = TokenUtils.GetParentOfType<Program>(this);
        Struct struct_ = program.GetStructFromType_(type_);
        List<Field> fields = struct_.GetFields();
        if (fields.Count != values.Count) {
            throw new SyntaxErrorException(
                $"{values.Count} values were supplied to an instantiation of struct {type_}, while {fields.Count} values are required.", this
            );
        }
        for (int i = 0; i < fields.Count; i++) {
            IValueToken value = values[i];
            Type_ valueType_ = value.GetType_();
            Type_ fieldType_ = fields[i].GetType_();
            if (!valueType_.IsConvertibleTo(fieldType_)) {
                throw new SyntaxErrorException(
                    $"Expected value of type {fieldType_} in instantiation, got value of type {valueType_}", value
                );
            }
        }
    }
}
