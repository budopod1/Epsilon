namespace Epsilon;
public class CodeSpan {
    readonly int start;
    readonly int end;

    public CodeSpan(int start, int end) {
        this.start = start;
        this.end = end;
    }

    public CodeSpan(int i) {
        start = i;
        end = i;
    }

    public int GetStart() {
        return start;
    }

    public int GetEnd() {
        return end;
    }

    public int Size() {
        return end - start + 1;
    }

    public static CodeSpan Merge(IEnumerable<CodeSpan> spans) {
        IEnumerable<CodeSpan> nonNull = spans.Where(
            span => span != null
        );
        if (nonNull.Count() == 0) return null;
        int start = nonNull.Select(span => span.GetStart()).Min();
        int end = nonNull.Select(span => span.GetEnd()).Max();
        return new CodeSpan(start, end);
    }

    public static CodeSpan Merge(CodeSpan a, CodeSpan b) {
        return Merge([a, b]);
    }

    public override string ToString() {
        return Utils.WrapName(
            GetType().Name,
            $"{start}–{end}"
        );
    }
}
