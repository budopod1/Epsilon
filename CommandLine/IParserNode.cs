using System;

public interface IParserNode {
    string GetContent();
    ParserTree GetParent();
}
