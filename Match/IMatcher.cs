using System;

public interface IMatcher {
    Match Match(TreeToken token);
}
