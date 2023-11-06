using System;

public class Name : Unit<string>, IVerifier {
    public Name(string name) : base(name) {}

    public void Verify() {
        throw new SyntaxErrorException(
            "No variable found with the name " + GetValue(), this
        );
    }
}
