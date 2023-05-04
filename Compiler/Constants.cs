using System;
using System.Collections.Generic;

public class Constants {
    Dictionary<int, Constant> constants = new Dictionary<int, Constant>();
    int counter = 0;

    public int AddConstant(Constant constant) {
        constants[counter] = constant;
        return counter++;
    }
}
