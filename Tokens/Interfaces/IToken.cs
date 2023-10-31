using System;

public interface IToken {
    IParentToken parent { get; set; }
    CodeSpan span { get; set; }
}
