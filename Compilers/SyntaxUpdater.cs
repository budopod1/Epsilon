using System.Text;

namespace Epsilon;
public static class SyntaxUpdater {
    static string GetLogFilePath() {
        return "updates.txt";
    }

    public static void Substitute(IToken from, object[] to) {
#if !UPGRADE_LOGGING
        return;
#endif
        StringBuilder builder = new();
        builder.Append(TokenUtils.GetParentOfType<Program>(from).GetRealPath());
        builder.Append('\t');
        builder.Append(from.span.GetStart());
        builder.Append('\t');
        builder.Append(from.span.GetEnd());
        builder.Append('\t');
        foreach (object replacement in to) {
            CodeSpan span = replacement as CodeSpan;
            if (replacement is IToken token) {
                span = token.span;
            }
            if (span != null) {
                builder.Append("span\t");
                builder.Append(span.GetStart());
                builder.Append('\t');
                builder.Append(span.GetEnd());
                builder.Append('\t');
            }
            if (replacement is string txt) {
                builder.Append("text\t");
                builder.Append(txt);
                builder.Append('\t');
            }
        }
        builder.Append('\n');
        File.AppendAllText(GetLogFilePath(), builder.ToString());
    }
}
