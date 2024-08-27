public class Name(string name) : Unit<string>(name), IVerifier {
    public void Verify() {
        throw new SyntaxErrorException(
            "No variable found with the name " + GetValue(), this
        );
    }
}
