using System;
using System.Collections.Generic;

public class Constants {
    Dictionary<int, IConstant> constants = new Dictionary<int, IConstant>();
    static int counter = 0;

    public int AddConstant(IConstant constant) {
        constants[counter] = constant;
        return counter++;
    }

    public IConstant GetConstant(int id) {
        return constants[id];
    }

    public override string ToString() {
        return string.Join("\n", constants);
    }
}
