public interface ISerializableToken : IToken {
    int Serialize(SerializationContext context);
}
