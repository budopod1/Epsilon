using System;

public interface IToken {
    IParentToken parent { get; set; }
}
