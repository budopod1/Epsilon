using System;
using System.Collections.Generic;

public class SerializationContext {
    Function function;
    JSONList instructions = new JSONList();
    CodeBlock block;
    int index;
    bool hidden;
    JSONList parentDeclarations;
    JSONList initialDeclarations = new JSONList();
    bool doesTerminateFunction = false;

    public SerializationContext(Function function, bool hidden=false, JSONList parentDeclarations=null) {
        if (parentDeclarations == null) parentDeclarations = new JSONList();
        this.parentDeclarations = parentDeclarations;
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
            if (token is Line) {
                Line line = ((Line)token);
                ICompleteLine instruction = (ICompleteLine)line[0];
                SerializeInstruction(instruction);
            }
        }
    }

    public SerializationContext AddSubContext(bool hidden=false) {
        JSONList declarations = new JSONList(parentDeclarations);
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
        JSONObject obj = new JSONObject();
        obj["instructions"] = instructions;
        obj["initial_declarations"] = initialDeclarations;
        obj["parent_declarations"] = parentDeclarations;
        obj["does_terminate_function"] = new JSONBool(doesTerminateFunction);
        return obj;
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
        if (function.Serialized.ContainsKey(token))
            return function.Serialized[token];
        int id = token.Serialize(this);
        function.Serialized[token] = id;
        return id;
    }
}
