namespace Lab0;

public class InputReader
{
    private readonly string _sourceCode;
    private int _position;
    private int _line = 1;
    private int _column;
    private bool? _usedCrLf; // Для отслеживания формата переноса строк

    private bool _inSingleQuote;
    private bool _inCurlyComment;

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
            // Проверка на незакрытые кавычки или скобки при достижении конца файла
            if (_inSingleQuote)
            {
                AddError("Незакрытая строковая константа до конца файла", ErrorCode.UnclosedStringOrComment);
            }
            if (_inCurlyComment)
            {
                AddError("Незакрытый блок комментария '{' до конца файла", ErrorCode.UnclosedStringOrComment);
            }

            CurrentChar = '\0';
            return CurrentChar;
        }

        // Проверка переносов CRLF / LF
        if (_sourceCode[_position] == '\r')
        {
            TrackLineEnding(isCrLf: true);
            _position++;
            if (IsEof)
            {
                CurrentChar = '\0';
                return CurrentChar;
            }
        }
        else if (_sourceCode[_position] == '\n')
        {
            TrackLineEnding(isCrLf: false);
        }

        // Подсчёт строк и колонок
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

        // --- ДИАГНОСТИКА ОШИБОК И ПРЕДУПРЕЖДЕНИЙ ---
        ValidateCurrentCharacter(CurrentChar);

        return CurrentChar;
    }

    private void ValidateCurrentCharacter(char ch)
    {
        // 1. Нулевой байт в тексте
        if (ch == '\0' && !IsEof)
        {
            AddError("Обнаружен нулевой байт (Null-byte)", ErrorCode.NullByteDetected);
        }

        // 2. Непечатные управляющие символы
        if (char.IsControl(ch) && ch is not ('\n' or '\r' or '\t'))
        {
            AddError($"Непечатный символ (ASCII код: {(int)ch})", ErrorCode.UnprintableCharacter);
        }

        // 3. Отслеживание контекста строковых литералов и комментариев
        if (ch == '\'' && !_inCurlyComment)
        {
            _inSingleQuote = !_inSingleQuote;
        }
        else if (ch == '{' && !_inSingleQuote)
        {
            _inCurlyComment = true;
        }
        else if (ch == '}' && _inCurlyComment)
        {
            _inCurlyComment = false;
        }

        // 4. Проверка кириллицы вне строк и комментариев
        if (!_inSingleQuote && !_inCurlyComment && ch >= '\u0400' && ch <= '\u04FF')
        {
            AddError($"Недопустимый кириллический символ '{ch}' в идентификаторе/коде", ErrorCode.CyrillicCharacter);
        }

        // 5. Превышение длины строки
        if (_column > 255)
        {
            AddError("Длина строки превышает 255 символов", ErrorCode.LineTooLong, ErrorSeverity.Warning);
        }
    }

    private void TrackLineEnding(bool isCrLf)
    {
        if (_usedCrLf.HasValue && _usedCrLf.Value != isCrLf)
        {
            AddError("Обнаружены смешанные переносы строк (CRLF и LF)", ErrorCode.MixedLineEndings, ErrorSeverity.Warning);
        }
        _usedCrLf = isCrLf;
    }

    public void AddError(string message, ErrorCode code = ErrorCode.UnprintableCharacter, ErrorSeverity severity = ErrorSeverity.Error)
    {
        Errors.Add(new CompilerError(_line, _column, message, code, severity));
    }
}