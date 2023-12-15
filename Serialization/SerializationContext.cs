using System;

public class SerializationContext {
    Function function;
    JSONList instructions = new JSONList();
    CodeBlock block;
    int index;
    bool hidden;
    JSONList parentAssignments;
    JSONList initialAssignments = new JSONList();

    public SerializationContext(Function function, bool hidden=false, JSONList parentAssignments=null) {
        if (parentAssignments == null) parentAssignments = new JSONList();
        this.parentAssignments = parentAssignments;
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
        JSONList assignments = new JSONList(parentAssignments);
        foreach (IJSONValue assignment in initialAssignments) {
            assignments.Add(assignment);
        }
        return new SerializationContext(
            function, hidden, assignments
        );
    }

    public int GetIndex() {
        return index;
    }

    public IJSONValue Serialize() {
        JSONObject obj = new JSONObject();
        obj["instructions"] = instructions;
        obj["initial_assignments"] = initialAssignments;
        obj["parent_assignments"] = parentAssignments;
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

    public void AddInitialAssignment(int varID) {
        initialAssignments.Add(new JSONInt(varID));
    }
}
