using System;
using System.Collections.Generic;

public interface IPatternProcessor<T> {
    T Process(List<Token> tokens, int start, int end);
}
