public interface ISerializableToken : IToken {
    int UncachedSerialize(SerializationContext context);
}
