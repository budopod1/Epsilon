using System;

public class Name : Unit<string>, IValueToken, IVerifier {
    public Name(string name) : base(name) {}

    public Type_ GetType_() {
        return Type_.Unknown(); // temp
    }

    public void Verify() {
        throw new SyntaxErrorException("No variable found with the name " + GetValue());
    }
}
