using System;
using System.Collections.Generic;

public class FunctionCall : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    Function function;
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
    
    public FunctionCall(Function function, List<IValueToken> arguments) {
        this.function = function;
        this.arguments = arguments;
    }

    public Type_ GetType_() {
        return function.GetReturnType_();
    }

    public override string ToString() {
        string title = Utils.WrapName(
            this.GetType().Name, String.Join(
                ", ", function.GetArguments().ConvertAll<string>(
                    obj => obj.ToString()
                )
            )
        );
        return Utils.WrapName(title, String.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        ));
    }
}
