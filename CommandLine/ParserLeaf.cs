using System;
using System.Collections.Generic;

public class ParserLeaf : IParserNode {
    string content;
    ParserTree parent;

    public ParserLeaf(string content, ParserTree parent) {
        this.content = content;
        this.parent = parent;
    }

    public ParserTree GetParent() {
        return parent;
    }

    public string GetContent() {
        return content;
    }
}
