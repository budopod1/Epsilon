public enum LogLevel {
    TEMP, // for statements that should be removed before commit
    INFO, // for standard info
    STATUS, // for status change (eg. now entering phase foo)
    WARN, // for something wrong but correctible (eg. InvalidSPECResourceException)
    ERROR, // for providing information on an uncorrectable problem
    NONE // do not pass to Log.Output
}
