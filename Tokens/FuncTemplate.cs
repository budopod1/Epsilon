using System;
using System.Collections.Generic;

public class FuncTemplate : Unit<PatternExtractor<List<IToken>>> {
    public FuncTemplate(PatternExtractor<List<IToken>> pattern) : base(pattern) {}
}
