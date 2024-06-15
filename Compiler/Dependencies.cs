using System;
using System.Linq;
using System.Collections.Generic;

public class Dependencies {
    IEnumerable<Struct> structs;
    IEnumerable<RealFunctionDeclaration> functions;

    public Dependencies(IEnumerable<Struct> structs, IEnumerable<RealFunctionDeclaration> functions) {
        this.structs = structs;
        this.functions = functions;
    }

    public static Dependencies Empty() {
        return new Dependencies(Enumerable.Empty<Struct>(), Enumerable.Empty<RealFunctionDeclaration>());
    }

    public IEnumerable<Struct> GetStructs() {
        return structs;
    }

    public IEnumerable<RealFunctionDeclaration> GetFunctions() {
        return functions;
    }
}