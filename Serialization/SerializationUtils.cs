using System;
using System.Runtime.Serialization;

public class SerializationUtils {
    public static ObjectIDGenerator idGenerator = new ObjectIDGenerator();

    public static long GetID(object o) {
        return idGenerator.GetId(o, out bool _);
    }
}
