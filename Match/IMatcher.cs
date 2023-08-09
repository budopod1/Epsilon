using System;

public interface IMatcher {
    Match Match(IParentToken token);
}
