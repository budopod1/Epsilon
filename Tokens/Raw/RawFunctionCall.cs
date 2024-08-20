using System;
using System.Collections.Generic;

public class RawFunctionCall(List<IPatternSegment> segments, List<IToken> arguments) : IParentToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }

    readonly HashSet<FunctionDeclaration> matchingFunctions = [];
    readonly List<IPatternSegment> segments = segments;
    readonly List<IToken> arguments = arguments;

    public int Count {
        get => arguments.Count;
    }

    public IToken this[int i] {
        get => arguments[i];
        set => arguments[i] = value;
    }

    public HashSet<FunctionDeclaration> GetMatchingFunctions() {
        return matchingFunctions;
    }

    public void AddMatchingFunction(FunctionDeclaration function) {
        matchingFunctions.Add(function);
    }

    public List<IPatternSegment> GetSegments() {
        return segments;
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, string.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        )) + $"(M: {matchingFunctions.Count})";
    }
}
