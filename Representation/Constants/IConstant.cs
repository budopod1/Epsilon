using CsJSONTools;

namespace Epsilon;
public interface IConstant {
    Type_ GetType_();
    bool IsTruthy();
    IJSONValue GetJSON();
}
