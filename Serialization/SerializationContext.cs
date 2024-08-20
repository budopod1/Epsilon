using System;
using System.Collections.Generic;

public class SerializationContext {
    readonly Function function;
    readonly JSONList instructions = [];
    CodeBlock block;
    readonly int index;
    readonly bool hidden;
    readonly JSONList parentDeclarations;
    readonly JSONList initialDeclarations = [];
    bool doesTerminateFunction = false;

    public SerializationContext(Function function, bool hidden=false, JSONList parentDeclarations=null) {
        this.parentDeclarations = parentDeclarations ?? [];
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
        doesTerminateFunction = block.DoesTerminateFunction();
        foreach (IToken token in block) {
            if (token is Line line) {
                ICompleteLine instruction = (ICompleteLine)line[0];
                SerializeInstruction(instruction);
            }
        }
    }

    public SerializationContext AddSubContext(bool hidden=false) {
        JSONList declarations = new(parentDeclarations);
        foreach (IJSONValue declaration in initialDeclarations) {
            declarations.Add(declaration);
        }
        return new SerializationContext(
            function, hidden, declarations
        );
    }

    public int GetIndex() {
        return index;
    }

    public IJSONValue Serialize() {
        return new JSONObject {
            ["instructions"] = instructions,
            ["initial_declarations"] = initialDeclarations,
            ["parent_declarations"] = parentDeclarations,
            ["does_terminate_function"] = new JSONBool(doesTerminateFunction)
        };
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

    public void AddDeclaration(int varID) {
        initialDeclarations.Add(new JSONInt(varID));
    }

    public int SerializeInstruction(ISerializableToken token) {
        if (function.Serialized.TryGetValue(token, out int value))
            return value;
        int id = token.Serialize(this);
        function.Serialized[token] = id;
        return id;
    }
}
