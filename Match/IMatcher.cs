using System;

public interface IMatcher {
    Match match(IToken token);
}
