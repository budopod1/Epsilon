using System;

public interface IMatcher {
    Match Match(IToken token);
}
