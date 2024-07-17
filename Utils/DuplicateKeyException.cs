using System;

public class DuplicateKeyException<TKey, TValue> : Exception {
    public TKey Key;
    public TValue Initial;
    public TValue Duplicate;

    public DuplicateKeyException(TKey key, TValue initial, TValue duplicate) {
        Key = key;
        Initial = initial;
        Duplicate = duplicate;
    }
}
