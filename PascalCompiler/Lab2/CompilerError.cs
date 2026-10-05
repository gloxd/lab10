namespace Lab2;

public enum ErrorSeverity
{
    Warning,
    Error,
    Fatal
}

public enum ErrorCode
{
    // Ошибки Lab0 / Lab1
    UnknownSymbol = 306,

    // Синтаксические ошибки Lab2
    ExpectedToken = 401,
    InvalidProgramHeader = 402,
    InvalidDeclaration = 403,
    InvalidExpression = 404,
    UnbalancedBeginEnd = 405,
    UnexpectedEof = 406
}

public record CompilerError(
    int Line,
    int Column,
    string Message,
    ErrorCode Code = ErrorCode.ExpectedToken,
    ErrorSeverity Severity = ErrorSeverity.Error
)
{
    public override string ToString()
    {
        string type = Severity switch
        {
            ErrorSeverity.Warning => "Предупреждение",
            ErrorSeverity.Fatal => "Критическая ошибка",
            _ => "Ошибка"
        };
        return $"[{type} ERR-{(int)Code}] (Стр {Line}, Кол {Column}): {Message}";
    }
}