using System;

public interface IMatcher {
    Match Match(ParentToken token);
}
