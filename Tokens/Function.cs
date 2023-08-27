using System;
using System.Collections.Generic;

public class Function : IParentToken {
    public IParentToken parent { get; set; }
    
    public int Count {
        get { return 1 + arguments.Count; }
    }
    
    public IToken this[int i] {
        get {
            if (i == 0) {
                return block;
            } else {
                return arguments[i-1];
            }
        }
        set {
            if (i == 0) {
                block = ((Block)value);
            } else {
                arguments[i-1] = (FunctionArgumentToken)value;
            }
        }
    }
    
    PatternExtractor<List<IToken>> pattern;
    List<FunctionArgumentToken> arguments;
    Block block;
    Scope scope = new Scope();
    Type_ returnType_;
    
    public Function(PatternExtractor<List<IToken>> pattern, 
                    List<FunctionArgumentToken> arguments, Block block,
                    Type_ returnType_) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.block = block;
        this.returnType_ = returnType_;
    }

    public PatternExtractor<List<IToken>> GetPattern() {
        return pattern;
    }

    public List<FunctionArgumentToken> GetArguments() {
        return arguments;
    }

    public Block GetBlock() {
        return block;
    }

    public void SetBlock(Block block) {
        this.block = block;
    }

    public Scope GetScope() {
        return scope;
    }

    public Type_ GetReturnType_() {
        return returnType_;
    }

    public override string ToString() {
        string title = Utils.WrapName(
            this.GetType().Name, String.Join(", ", arguments), "<", ">"
        );
        return Utils.WrapName(title, block.ToString());
    }
}
