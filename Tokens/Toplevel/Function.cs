using System;
using System.Linq;
using System.Collections.Generic;

public class Function : IParentToken, ITopLevel, IVerifier {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    static int id_ = 0;
    
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
                block = ((CodeBlock)value);
            } else {
                arguments[i-1] = (FunctionArgumentToken)value;
            }
        }
    }
    
    PatternExtractor<List<IToken>> pattern;
    List<FunctionArgumentToken> arguments;
    CodeBlock block;
    Scope scope = new Scope();
    Type_ returnType_;
    int id;
    List<SerializationContext> contexts = new List<SerializationContext>();
    
    public Function(PatternExtractor<List<IToken>> pattern, 
                    List<FunctionArgumentToken> arguments, CodeBlock block,
                    Type_ returnType_) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.block = block;
        this.returnType_ = returnType_;
        foreach (FunctionArgumentToken argument in arguments) {
            argument.SetID(scope.AddVar(
                argument.GetName(), argument.GetType_()
            ));
        }
        id = id_++;
    }

    public PatternExtractor<List<IToken>> GetPattern() {
        return pattern;
    }

    public List<FunctionArgumentToken> GetArguments() {
        return arguments;
    }

    public CodeBlock GetBlock() {
        return block;
    }

    public void SetBlock(CodeBlock block) {
        this.block = block;
    }

    public Scope GetScope() {
        return scope;
    }

    public Type_ GetReturnType_() {
        return returnType_;
    }

    public int GetID() {
        return id;
    }

    public override string ToString() {
        string title = Utils.WrapName(
            GetType().Name, String.Join(", ", arguments), "<", ">"
        );
        return Utils.WrapName(title, block.ToString());
    }

    public int RegisterContext(SerializationContext context) {
        contexts.Add(context);
        return contexts.Count-1;
    }

    public int? GetContextIdByBlock(CodeBlock block) {
        int i = 0;
        foreach (SerializationContext context in contexts) {
            if (context.GetBlock() == block) {
                return i;
            }
            i++;
        }
        return null;
    }

    public IJSONValue GetJSON() {
        JSONObject obj = new JSONObject();
        obj["id"] = new JSONInt(id);
        obj["arguments"] = new JSONList(arguments.Select(
            argument => argument.GetJSON()
        ));
        obj["return_type_"] = returnType_.GetJSON();
        new SerializationContext(this).Serialize(block);
        obj["instructions"] = new JSONList(contexts.Select(
            context=>context.GetInstructions()
        ));
        obj["scope"] = scope.GetJSON();
        return obj;
    }

    public void Verify() {
        if (!returnType_.GetBaseType_().IsVoid()) {
            if (block.Count == 0) {
                throw new SyntaxErrorException(
                    "Functions with a non-void return value cannot be empty", block
                );
            }
            IToken token = block[block.Count-1];
            Line line = token as Line;
            if (line == null) return;
            line.Verify(); // make sure line.Count == 1
            if (!(line[0] is Return)) {
                throw new SyntaxErrorException(
                    "Functions with a non-void return value must end with a return statement", line
                );
            }
        }
    }
}
