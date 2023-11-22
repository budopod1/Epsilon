using System;

public class SerializationContext {
    Function function;
    JSONList instructions = new JSONList();
    CodeBlock block;
    int index;
    bool hidden;

    public SerializationContext(Function function, bool hidden=false) {
        this.function = function;
        this.hidden = hidden;
        if (!hidden) {
            index = function.RegisterContext(this);
        }
    }

    public int AddInstruction(SerializableInstruction instruction) {
        instructions.Add(instruction.GetJSON());
        return instructions.Count-1;
    }

    public void Serialize(CodeBlock block) {
        this.block = block;
        foreach (IToken token in block) {
            if (token is Line) {
                Line line = ((Line)token);
                ICompleteLine instruction = (ICompleteLine)line[0];
                instruction.Serialize(this);
            }
        }
    }

    public SerializationContext AddSubContext(bool hidden=false) {
        return new SerializationContext(function, hidden);
    }

    public int GetIndex() {
        return index;
    }

    public IJSONValue GetInstructions() {
        return instructions;
    }

    public CodeBlock GetBlock() {
        return block;
    }

    public Function GetFunction() {
        return function;
    }

    public bool IsHidden() {
        return hidden;
    }
}
