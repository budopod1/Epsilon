using System;
using System.Collections.Generic;

public class FunctionCall : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    FunctionDeclaration function;
    List<IValueToken> arguments;
    
    public int Count {
        get {
            return arguments.Count;
        }
    }
    
    public IToken this[int i] {
        get {
            return arguments[i];
        }
        set {
            arguments[i] = (IValueToken)value;
        }
    }
    
    public FunctionCall(FunctionDeclaration function, List<IValueToken> arguments) {
        this.function = function;
        this.arguments = arguments;
    }

    public Type_ GetType_() {
        return function.GetReturnType_(arguments);
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, String.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        ));
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this, context)
                .AddData("function", new JSONString(function.GetID()))
        );
    }
}
