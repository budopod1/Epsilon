using System;
using System.Collections.Generic;

public interface IPatternProcessor<T> {
    T Process(List<IToken> tokens, int start, int end);
}
