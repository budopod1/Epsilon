namespace Epsilon;

public class PackagingProblemException(string message, Package package = null) : Exception(message) {
    readonly public Package Package_ = package;
}
