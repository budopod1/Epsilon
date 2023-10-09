using System;
using System.Collections.Generic;

abstract public class AdvancedPatternExtractor<T> : ITokenExtractor<T> {
    enum Part {
        Start,
        Repeated,
        End
    }
    
    protected List<IPatternSegment> start;
    protected List<IPatternSegment> repeated;
    protected int minRepeats;
    protected int maxRepeats;
    protected List<IPatternSegment> end;
    protected IPatternProcessor<T> processor;
    
    Action<List<IToken>, int, int> callback;

    public void SetCallback(Action<List<IToken>, int, int> callback) {
        this.callback = callback;
    }

    public List<IPatternSegment> GetStartSegments() {
        return start;
    }

    public int GetMinRepeats() {
        return minRepeats;
    }

    public int GetMaxRepeats() {
        return maxRepeats;
    }

    public List<IPatternSegment> GetRepeatedSegments() {
        return repeated;
    }

    public List<IPatternSegment> GetEndSegments() {
        return end;
    }

    public T Extract(IParentToken tokens) {
        Part part = Part.Start;
        for (int i = 0; i < tokens.Count; i++) {
            bool finishedMatch = false;
            int j;
            int lastRepeatStop = -2; // should always be assigned before refrence
            int pi = 0; // part index
            int repeats = 0;
            List<IToken> tokenList = new List<IToken>();
            List<IToken> repeatPartList = new List<IToken>();
            bool spaceTermination = true;
            for (j = 0; (i+j) < tokens.Count; j++) {
                IToken token = tokens[i+j];
                if (part == Part.Start) {
                    if (start.Count == 0) {
                        j--; // back it up, as this doesn't count
                        part = Part.Repeated;
                        lastRepeatStop = j;
                        continue;
                    }
                    IPatternSegment segment = start[pi];
                    if (segment.Matches(token)) {
                        tokenList.Add(token);
                        pi++;
                        if (pi == start.Count) {
                            pi = 0;
                            part = Part.Repeated;
                            lastRepeatStop = j;
                        }
                        continue;
                    } else {
                        spaceTermination = false;
                        break;
                    }
                } else if (part == Part.Repeated) {
                    if (repeated.Count == 0) {
                        part = Part.End;
                        j--; // back it up, as this doesn't count
                        continue;
                    }
                    IPatternSegment segment = repeated[pi];
                    if (segment.Matches(token)) {
                        repeatPartList.Add(token);
                        pi++;
                        if (pi == repeated.Count) {
                            pi = 0;
                            tokenList.AddRange(repeatPartList);
                            repeats++;
                            if (repeats == maxRepeats) {
                                part = Part.End;
                                continue;
                            }
                            repeatPartList = new List<IToken>();
                            lastRepeatStop = j;
                        }
                        continue;
                    } else {
                        if (repeats >= minRepeats) {
                            pi = 0;
                            j = lastRepeatStop;
                            part = Part.End;
                            continue;
                        } else {
                            spaceTermination = false;
                            break;
                        }
                    }
                } else if (part == Part.End) {
                    if (end.Count == 0) {
                        j--; // back it up, as this doesn't count
                        finishedMatch = true;
                        spaceTermination = false;
                        break;
                    }
                    IPatternSegment segment = repeated[pi];
                    if (segment.Matches(token)) {
                        tokenList.Add(token);
                        pi++;
                        if (pi == end.Count) {
                            finishedMatch = true;
                            spaceTermination = false;
                            break;
                        }
                    } else {
                        spaceTermination = false;
                        break;
                    }
                }
            }
            if (part == Part.End && end.Count == 0)
                finishedMatch = true;
            if (part == Part.Repeated && end.Count == 0 
                && spaceTermination)
                finishedMatch = true;
            if (finishedMatch) {
                if (callback != null) {
                    callback(tokenList, i, i+j-1);
                    callback = null;
                }
                return processor.Process(tokenList, i, i+j);
            }
        }
        callback = null;
        return default(T);
    }
}
