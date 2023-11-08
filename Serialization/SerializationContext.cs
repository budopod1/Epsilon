using System;

public class SerializationContext {
    Function function;
    JSONList instructions = new JSONList();
    int index;

    public SerializationContext(Function function) {
        this.function = function;
        index = function.RegisterContext(this);
    }

    public int AddInstruction(SerializableInstruction instruction) {
        instructions.Add(instruction.GetJSON());
        return instructions.Count-1;
    }

    public void Serialize(CodeBlock block) {
        foreach (IToken token in block) {
            if (token is Line) {
                Line line = ((Line)token);
                ICompleteLine instruction = (ICompleteLine)line[0];
                instruction.Serialize(this);
            }
        }
    }

    public SerializationContext AddSubContext() {
        return new SerializationContext(function);
    }

    public int GetIndex() {
        return index;
    }

    public IJSONValue GetInstructions() {
        return instructions;
    }
}
