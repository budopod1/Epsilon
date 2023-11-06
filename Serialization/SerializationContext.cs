using System;

public class SerializationContext {
    IToken context;
    JSONList instructions = new JSONList();

    public SerializationContext(IToken context) {
        this.context = context;
    }

    public int AddInstruction(SerializableInstruction instruction) {
        instructions.Add(instruction.GetJSON());
        return instructions.Count-1;
    }

    public IJSONValue GetInstructions() {
        return instructions;
    }
}
