using CsJSONTools;

public interface IConstant {
    Type_ GetType_();
    bool IsTruthy();
    IJSONValue GetJSON();
}
