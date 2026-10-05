namespace Lab2;

public class InputReader
{
    private readonly string _sourceCode;
    private int _position;
    private int _line = 1;
    private int _column;

    public char CurrentChar { get; private set; }
    public int Line => _line;
    public int Column => _column;
    public bool IsEof => _position >= _sourceCode.Length;

    public List<CompilerError> Errors { get; } = new();

    public InputReader(string sourceCode)
    {
        _sourceCode = sourceCode ?? string.Empty;
        _position = 0;
        _column = 0;

        NextCh();
    }

    public static InputReader FromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Файл не найден: {filePath}");
        }

        return new InputReader(File.ReadAllText(filePath));
    }

    public char NextCh()
    {
        if (IsEof)
        {
            CurrentChar = '\0';
            return CurrentChar;
        }

        if (_sourceCode[_position] == '\r')
        {
            _position++;
            if (IsEof)
            {
                CurrentChar = '\0';
                return CurrentChar;
            }
        }

        if (_position > 0 && _sourceCode[_position - 1] == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            if (_sourceCode[_position] == '\t')
            {
                _column += 4;
            }
            else
            {
                _column++;
            }
        }

        CurrentChar = _sourceCode[_position];
        _position++;

        return CurrentChar;
    }

    public void AddError(string message, ErrorCode code = ErrorCode.ExpectedToken, ErrorSeverity severity = ErrorSeverity.Error)
    {
        Errors.Add(new CompilerError(_line, _column, message, code, severity));
    }
}