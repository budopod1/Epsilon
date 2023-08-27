using System;
using System.Collections.Generic;

public class RawFunctionCall : IParentToken {
    public IParentToken parent { get; set; }
    
    HashSet<Function> matchingFunctions = new HashSet<Function>();
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

    public HashSet<Function> GetMatchingFunctions() {
        return matchingFunctions;
    }

    public void AddMatchingFunction(Function function) {
        matchingFunctions.Add(function);
    }

    public List<IPatternSegment> GetSegments() {
        return segments;
    }

    public Type_ GetType_() {
        return Type_.Unknown(); // temp
    }

    public override string ToString() {
        return Utils.WrapName(this.GetType().Name, String.Join(
            ", ", arguments.ConvertAll<string>(obj => obj.ToString())
        )) + $"(M: {matchingFunctions.Count})";
    }
}
