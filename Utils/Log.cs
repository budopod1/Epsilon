namespace Epsilon;
public static class Log {
    public static LogLevel Verbosity = LogLevel.NONE;

    public static void Tmp(params object[] vals) {
        Output(LogLevel.TEMP, vals);
    }

    public static void Info(params object[] vals) {
        Output(LogLevel.INFO, vals);
    }

    public static void Status(params object[] vals) {
        Output(LogLevel.STATUS, vals);
    }

    public static void Warning(params object[] vals) {
        Output(LogLevel.WARN, vals);
    }

    public static void Error(params object[] vals) {
        Output(LogLevel.ERROR, vals);
    }

    public static void Output(LogLevel level, params object[] vals) {
        if (level < Verbosity) return;

        TextWriter stream = Console.Out;
        if (level == LogLevel.WARN || level == LogLevel.ERROR) {
            stream = Console.Error;
        }

        stream.Write(level.ToString().PadRight(6) + ":");

        for (int i = 0; i < vals.Length; i++) {
            if (i != 0) stream.Write(' ');
            stream.Write(vals[i]?.ToString() ?? "null");
        }

        stream.WriteLine();
    }
}
