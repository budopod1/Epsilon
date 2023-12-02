using System;
using System.Collections.Generic;

public class RawFunctionCall : IParentToken {
    public IParentToken parent { get; set; }
    public CodeSpan span { get; set; }
    
    HashSet<IFunctionDeclaration> matchingFunctions = new HashSet<IFunctionDeclaration>();
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

    public HashSet<IFunctionDeclaration> GetMatchingFunctions() {
        return matchingFunctions;
    }

    public void AddMatchingFunction(IFunctionDeclaration function) {
        matchingFunctions.Add(function);
    }

    public List<IPatternSegment> GetSegments() {
        return segments;
    }

    public Type_ GetType_() {
        return Type_.Unknown(); // temp
    }

    public override string ToString() {
        return Utils.WrapName(GetType().Name, String.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        )) + $"(M: {matchingFunctions.Count})";
    }
}
