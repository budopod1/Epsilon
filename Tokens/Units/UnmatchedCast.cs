public class UnmatchedCast : Unit<Type_>, IVerifier {
    public UnmatchedCast(Type_ type_) : base(type_) {}
    public UnmatchedCast(Type_Token type_Token) : base(type_Token.GetValue()) {}

    public void Verify() {
        throw new SyntaxErrorException(
            "Unmatched cast", this
        );
    }
}
