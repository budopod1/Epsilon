public class DuplicateKeyException<TKey, TValue>(TKey key, TValue initial, TValue duplicate) : Exception {
    public TKey Key = key;
    public TValue Initial = initial;
    public TValue Duplicate = duplicate;
}
