using System;
using System.Collections.Generic;

public class RawFunctionCall : IParentToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    HashSet<FunctionDeclaration> matchingFunctions = new HashSet<FunctionDeclaration>();
    List<IPatternSegment> segments;
    List<IToken> arguments;
    
    public int Count {
        get {
            return arguments.Count;
        }
    }
    
    public IToken this[int i] {
        get {
            return arguments[i];
        }
        set {
            arguments[i] = (IToken)value;
        }
    }
    
    public RawFunctionCall(List<IPatternSegment> segments, List<IToken> arguments) {
        this.segments = segments;
        this.arguments = arguments;
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
        return Utils.WrapName(GetType().Name, String.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        )) + $"(M: {matchingFunctions.Count})";
    }
}
