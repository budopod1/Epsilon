namespace Epsilon;
public class SyntaxErrorException : Exception {
    public CodeSpan span;

    public SyntaxErrorException(string message, IToken token) : base(message) {
        if (token.span == null) {
            Console.WriteLine(message);
            Console.WriteLine(token);
            throw new NullReferenceException("IToken.span should never be null");
        }
        span = token.span;
    }

    public SyntaxErrorException(string message, CodeSpan span) : base(message) {
        this.span = span;
    }
}
