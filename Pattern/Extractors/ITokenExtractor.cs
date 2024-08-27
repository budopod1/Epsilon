public interface ITokenExtractor<T> {
    T Extract(IParentToken tokens);
}
