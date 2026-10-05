namespace Lab1;

public enum ErrorSeverity
{
    Warning,
    Error,
    Fatal
}

public enum ErrorCode
{
    InvalidIdentifier = 301,
    IntegerOverflow = 302,
    InvalidNumberFormat = 303,
    UnterminatedComment = 304,
    UnterminatedString = 305,
    UnknownSymbol = 306,
    IdentifierTooLong = 307
}

public record CompilerError(
    int Line,
    int Column,
    string Message,
    ErrorCode Code = ErrorCode.UnknownSymbol,
    ErrorSeverity Severity = ErrorSeverity.Error
)
{
    public override string ToString()
    {
        string type = Severity == ErrorSeverity.Warning ? "Предупреждение" : "Ошибка";
        return $"[{type} ERR-{(int)Code}] (Стр {Line}, Кол {Column}): {Message}";
    }
}