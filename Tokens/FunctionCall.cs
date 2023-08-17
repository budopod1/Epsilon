using System;
using System.Collections.Generic;

public class FunctionCall : IParentToken, IValueToken {
    public IParentToken parent { get; set; }
    
    Name function;
    List<IValueToken> arguments;
    
    public int Count {
        get {
            return 1 + arguments.Count;
        }
    }
    
    public IToken this[int i] {
        get {
            if (i == 0) return function;
            return arguments[i-1];
        }
        set {
            if (i == 0) function = (Name)value;
            arguments[i-1] = (IValueToken)value;
        }
    }
    
    public FunctionCall(Name function, List<IValueToken> arguments) {
        this.function = function;
        this.arguments = arguments;
    }

    public Type_ GetType_() {
        return Type_.Unknown(); // temp
    }

    public override string ToString() {
        string title = Utils.WrapName(
            this.GetType().Name, function.ToString()
        );
        return Utils.WrapName(title, String.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        ));
    }
}
