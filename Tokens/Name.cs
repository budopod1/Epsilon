using System;

public class Name : Unit<string>, IValueToken {
    public Name(string name) : base(name) {}

    public Type_ GetType_() {
        return Type_.Unknown(); // temp
    }
}
