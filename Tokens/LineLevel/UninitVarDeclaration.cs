using System;
using System.Collections.Generic;

public class UninitVarDeclaration : ICompleteLine, ISerializableToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    int id;
    
    public UninitVarDeclaration(VarDeclaration declaration) {
        id = declaration.GetID();
    }

    public int Serialize(SerializationContext context) {
        return context.AddInstruction(
            new SerializableInstruction(this).AddData("variable", new JSONInt(id))
        );
    }
}
