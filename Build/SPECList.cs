using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

public class SPECList : Collection<ISPECVal>, ISPECVal {
    public SPECList(IEnumerable<ISPECVal> values) {
        foreach (ISPECVal value in values) Add(value);
    }

    public SPECList() {}

    public override string ToString() {
        IEnumerable<string> strings = this.Select(item => item.ToString());
        return $"[\n{Utils.Indent(String.Join("\n", strings))}\n]";
    }
}
