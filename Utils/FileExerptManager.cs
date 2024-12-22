namespace Epsilon;
public class FileExerptManager(string src) {
    readonly string src = src;
    readonly string[] lines = src.Split('\n');

    public int GetLineIdxFromPos(int pos) {
        return src[..pos].Count(chr => chr == '\n');
    }

    public string GetLineFromIdx(int idx) {
        return lines[idx];
    }

    public int GetLineBaseIdx(int lineIdx) {
        IEnumerable<int> newlineIdxs = new int[] {-1}.Concat(
            src.Select((chr, idx) => chr == '\n' ? idx : -1).Where(idx => idx != -1));
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(lineIdx, newlineIdxs.Count(), nameof(lineIdx));
        return newlineIdxs.ElementAt(lineIdx)+1;
    }

    public int GetTextIndent(string txt) {
        return txt.TakeWhile(char.IsWhiteSpace).Count();
    }

    public string GetLinesWithContext(int startIdx, int endIdx, int context) {
        int startLine = Math.Max(0, GetLineIdxFromPos(startIdx)-context);
        int endLine = Math.Min(lines.Length-1, GetLineIdxFromPos(endIdx)+context);
        return string.Join('\n', lines[startLine..(endLine+1)]);
    }

    public string AnnotateLine(int startIdx, int endIdx, string start, string middle, string end) {
        int lineIdx = GetLineIdxFromPos(startIdx);
        int lineBaseIdx = GetLineBaseIdx(lineIdx) + GetTextIndent(GetLineFromIdx(lineIdx));
        string spacer = new(' ', startIdx-lineBaseIdx);
        string pointer = startIdx == endIdx ? middle :
            start+string.Concat(Enumerable.Repeat(middle, endIdx-startIdx-1))+end;
        return spacer + pointer;
    }

    public string AnnotateLines(int startIdx, int endIdx, int context, string none, string start, string middle, string end) {
        int startLineIdx = GetLineIdxFromPos(startIdx);
        int endLineIdx = GetLineIdxFromPos(endIdx);
        int startLine = Math.Max(0, startLineIdx-context);
        int endLine = Math.Min(lines.Length-1, endLineIdx+context);
        IEnumerable<(string, int)> selected = lines
            .Select((line, idx) => (line, idx))
            .ToList()[startLine..(endLine+1)];
        int commonIndent = selected.Where(pair => pair.Item1.Trim() != "")
            .Min(pair => GetTextIndent(pair.Item1));
        return string.Join('\n', selected.Select(
            pair => {
                (string line, int idx) = pair;
                return (
                    idx < startLineIdx || idx > endLineIdx ? none
                    : idx == startLineIdx ? start
                    : idx == endLineIdx ? end
                    : middle
                ) + (line.Length > commonIndent ? line[commonIndent..] : "");
            }
        ));
    }
}
