using System;
using System.Reflection;
using System.Collections.Generic;

public class InstantiationPatternProcessor : IPatternProcessor<List<IToken>> {
    Type type;

    public InstantiationPatternProcessor(Type type) {
        this.type = type;
    }

    public List<IToken> Process(List<IToken> tokens, int start, int end) {
        IToken result = (IToken)Activator.CreateInstance(type);
        return new List<IToken> {result};
    }
}
