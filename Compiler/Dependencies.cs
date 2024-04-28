using System;
using System.Collections.Generic;

public class Dependencies {
    List<Struct> structs;
    List<RealFunctionDeclaration> functions;

    public Dependencies(List<Struct> structs, List<RealFunctionDeclaration> functions) {
        this.structs = structs;
        this.functions = functions;
    }

    public List<Struct> GetStructs() {
        return structs;
    }

    public List<RealFunctionDeclaration> GetFunctions() {
        return functions;
    }
}