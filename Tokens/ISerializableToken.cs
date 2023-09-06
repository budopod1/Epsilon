using System;

public interface ISerializableToken : IToken {
    IJSONValue Serialize();
}
