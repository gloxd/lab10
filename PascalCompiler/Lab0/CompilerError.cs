namespace Lab0;

public enum ErrorSeverity
{
    Warning,
    Error,
    Fatal
}

public enum ErrorCode
{
    UnprintableCharacter = 201,
    CyrillicCharacter = 202,
    MixedLineEndings = 203,
    LineTooLong = 204,
    UnclosedStringOrComment = 205,
    NullByteDetected = 206
}

public record CompilerError(
    int Line,
    int Column,
    string Message,
    ErrorCode Code = ErrorCode.UnprintableCharacter,
    ErrorSeverity Severity = ErrorSeverity.Error
)
{
    public override string ToString()
    {
        string type = Severity == ErrorSeverity.Warning ? "Предупреждение" : "Ошибка";
        return $"[{type} ERR-{(int)Code}] (Строка {Line}, Колонка {Column}): {Message}";
    }
}