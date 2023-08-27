using System;
using System.Collections.Generic;

public interface ITokenExtractor<T> {
    T Extract(IParentToken tokens);
}
