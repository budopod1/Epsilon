using System;
using System.Linq;
using System.Collections.Generic;

public class ArrayCreation : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    
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
    
    public ArrayCreation(Type_ type_, List<IValueToken> values) {
        this.type_ = type_;
        this.values = values;
    }
    
    public ArrayCreation(ValueList list) {
        this.values = list.GetValues().Select(
            (ValueListItem token) => (IValueToken)token[0]
        ).ToList();
        this.type_ = values[0].GetType_();
        foreach (IValueToken value in values) {
            if (!value.GetType_().Equals(type_)) {
                throw new SyntaxErrorException(
                    "Arrays cannot contain multiple types"
                );
            }
        }
    }

    public Type_ GetType_() {
        return new Type_("Array", new List<Type_> {type_});
    }

    public override string ToString() {
        return Utils.WrapName(
            Utils.WrapName(
                this.GetType().Name,
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
}
