using System;
using System.Collections.Generic;

public class Function : Token, IMultiLineToken {
    PatternExtractor<List<Token>> pattern;
    List<FunctionArgumentToken> arguments;
    Block block;
    
    public Function(PatternExtractor<List<Token>> pattern, 
                         List<FunctionArgumentToken> arguments, Block block) {
        this.pattern = pattern;
        this.arguments = arguments;
        this.block = block;
    }

    public PatternExtractor<List<Token>> GetPattern() {
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

    public override string ToString() {
        string title = Utils.WrapName(
            this.GetType().Name, String.Join(", ", arguments), "<", ">"
        );
        return Utils.WrapName(title, block.ToString());
    }
}
