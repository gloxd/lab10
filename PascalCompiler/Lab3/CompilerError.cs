namespace Lab3;

public enum ErrorSeverity
{
    Warning,
    Error,
    Fatal
}

public enum ErrorCode
{
    // Семантические ошибки Lab3
    RedeclaredIdentifier = 501,
    UndeclaredIdentifier = 502,
    TypeMismatch = 503,
    UninitializedVariable = 504,
    CannotAssignToConstant = 505
}

public record CompilerError(
    int Line,
    int Column,
    string Message,
    ErrorCode Code = ErrorCode.UndeclaredIdentifier,
    ErrorSeverity Severity = ErrorSeverity.Error
)
{
    public override string ToString()
    {
        string type = Severity == ErrorSeverity.Warning ? "Предупреждение" : "Ошибка";
        return $"[{type} ERR-{(int)Code}] (Стр {Line}, Кол {Column}): {Message}";
    }
}