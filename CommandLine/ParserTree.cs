using System;
using System.Collections.Generic;

public class ParserTree : IParserNode {
    List<IParserNode> nodes = new List<IParserNode>();
    string content;
    ParserTree parent;

    public ParserTree(string content, ParserTree parent) {
        this.content = content;
        this.parent = parent;
    }

    public ParserTree GetParent() {
        return parent;
    }

    public string GetContent() {
        return content;
    }

    public List<IParserNode> GetNodes() {
        return nodes;
    }

    public void AddNode(IParserNode node) {
        nodes.Add(node);
    }
}
