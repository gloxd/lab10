using System.Text;

namespace Lab1;

public class Lexer
{
    private readonly InputReader _reader;
    private const int MaxIdentifierLength = 63;

    private static readonly Dictionary<string, TokenType> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        { "program", TokenType.Program },
        { "var", TokenType.Var },
        { "const", TokenType.Const },
        { "begin", TokenType.Begin },
        { "end", TokenType.End },
        { "if", TokenType.If },
        { "then", TokenType.Then },
        { "else", TokenType.Else },
        { "case", TokenType.Case },
        { "of", TokenType.Of },
        { "for", TokenType.For },
        { "to", TokenType.To },
        { "downto", TokenType.Downto },
        { "do", TokenType.Do }
    };

    public Lexer(InputReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    public Token NextToken()
    {
        SkipWhitespaceAndComments();

        if (_reader.IsEof)
        {
            return new Token(TokenType.Eof, string.Empty, _reader.Line, _reader.Column);
        }

        int startLine = _reader.Line;
        int startColumn = _reader.Column;
        char ch = _reader.CurrentChar;

        // 1. Идентификаторы и ключевые слова
        if (char.IsLetter(ch) || ch == '_')
        {
            string word = ReadIdentifier();

            if (word.Length > MaxIdentifierLength)
            {
                _reader.AddError(
                    $"Идентификатор '{word[..10]}...' превышает максимальную длину в {MaxIdentifierLength} символов.",
                    ErrorCode.IdentifierTooLong,
                    ErrorSeverity.Warning
                );
            }

            TokenType type = Keywords.TryGetValue(word, out TokenType keywordType)
                ? keywordType
                : TokenType.Identifier;

            return new Token(type, word, startLine, startColumn);
        }

        // 2. Числовые литералы и проверка на склеивание с буквами (12abc)
        if (char.IsDigit(ch))
        {
            string numberStr = ReadNumber();

            // Проверяем, не идет ли сразу за числом буква (ошибка типа 123var)
            if (char.IsLetter(_reader.CurrentChar) || _reader.CurrentChar == '_')
            {
                string invalidTail = ReadIdentifier();
                _reader.AddError(
                    $"Некорректный идентификатор/число '{numberStr}{invalidTail}': идентификатор не может начинаться с цифр.",
                    ErrorCode.InvalidIdentifier
                );
                return new Token(TokenType.Unknown, numberStr + invalidTail, startLine, startColumn);
            }

            // Проверка переполнения Integer (диапазон -2147483648 ... 2147483647)
            if (!long.TryParse(numberStr, out long value) || value < int.MinValue || value > int.MaxValue)
            {
                _reader.AddError(
                    $"Число '{numberStr}' выходит за пределы допустимого диапазона 32-битного Integer.",
                    ErrorCode.IntegerOverflow
                );
            }

            return new Token(TokenType.IntegerLiteral, numberStr, startLine, startColumn);
        }

        // 3. Строковые константы (напр. 'hello')
        if (ch == '\'')
        {
            string strLiteral = ReadStringLiteral(startLine, startColumn);
            return new Token(TokenType.Identifier, strLiteral, startLine, startColumn); // или TokenType.StringLiteral если есть
        }

        // 4. Операторы и пунктуация
        switch (ch)
        {
            case ';': _reader.NextCh(); return new Token(TokenType.Semicolon, ";", startLine, startColumn);
            case ':':
                _reader.NextCh();
                if (_reader.CurrentChar == '=')
                {
                    _reader.NextCh();
                    return new Token(TokenType.Assign, ":=", startLine, startColumn);
                }
                return new Token(TokenType.Colon, ":", startLine, startColumn);
            case ',': _reader.NextCh(); return new Token(TokenType.Comma, ",", startLine, startColumn);
            case '.': _reader.NextCh(); return new Token(TokenType.Dot, ".", startLine, startColumn);
            case '=': _reader.NextCh(); return new Token(TokenType.Equal, "=", startLine, startColumn);
            case '+': _reader.NextCh(); return new Token(TokenType.Plus, "+", startLine, startColumn);
            case '-': _reader.NextCh(); return new Token(TokenType.Minus, "-", startLine, startColumn);
            case '*': _reader.NextCh(); return new Token(TokenType.Star, "*", startLine, startColumn);
            case '/': _reader.NextCh(); return new Token(TokenType.Slash, "/", startLine, startColumn);
            default:
                _reader.AddError($"Неизвестный лексический символ '{ch}'", ErrorCode.UnknownSymbol);
                _reader.NextCh();
                return new Token(TokenType.Unknown, ch.ToString(), startLine, startColumn);
        }
    }

    private void SkipWhitespaceAndComments()
    {
        while (!_reader.IsEof)
        {
            if (char.IsWhiteSpace(_reader.CurrentChar))
            {
                _reader.NextCh();
            }
            else if (_reader.CurrentChar == '{')
            {
                int startLine = _reader.Line;
                int startColumn = _reader.Column;

                _reader.NextCh();
                while (!_reader.IsEof && _reader.CurrentChar != '}')
                {
                    _reader.NextCh();
                }

                if (_reader.IsEof)
                {
                    _reader.AddError("Незакрытый блок комментария '{'", ErrorCode.UnterminatedComment);
                    break;
                }

                _reader.NextCh(); // Пропускаем '}'
            }
            else
            {
                break;
            }
        }
    }

    private string ReadIdentifier()
    {
        var sb = new StringBuilder();
        while (!_reader.IsEof && (char.IsLetterOrDigit(_reader.CurrentChar) || _reader.CurrentChar == '_'))
        {
            sb.Append(_reader.CurrentChar);
            _reader.NextCh();
        }
        return sb.ToString();
    }

    private string ReadNumber()
    {
        var sb = new StringBuilder();
        while (!_reader.IsEof && char.IsDigit(_reader.CurrentChar))
        {
            sb.Append(_reader.CurrentChar);
            _reader.NextCh();
        }
        return sb.ToString();
    }

    private string ReadStringLiteral(int startLine, int startColumn)
    {
        var sb = new StringBuilder();
        _reader.NextCh(); // пропускаем открывающую кавычку '

        while (!_reader.IsEof && _reader.CurrentChar != '\'')
        {
            if (_reader.CurrentChar == '\n' || _reader.CurrentChar == '\r')
            {
                _reader.AddError("Перенос строки внутри строковой константы запрещен.", ErrorCode.UnterminatedString);
                return sb.ToString();
            }
            sb.Append(_reader.CurrentChar);
            _reader.NextCh();
        }

        if (_reader.IsEof)
        {
            _reader.AddError("Незакрытая строковая константа до конца файла.", ErrorCode.UnterminatedString);
        }
        else
        {
            _reader.NextCh(); // пропускаем закрывающую кавычку '
        }

        return sb.ToString();
    }
}