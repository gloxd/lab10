using System.Text;

namespace Lab2;

public class Lexer
{
    private readonly InputReader _reader;

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
        { "do", TokenType.Do },
        { "integer", TokenType.IntegerType },
        { "boolean", TokenType.BooleanType },
        { "true", TokenType.BooleanLiteral },
        { "false", TokenType.BooleanLiteral }
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

        if (char.IsLetter(ch) || ch == '_')
        {
            string word = ReadIdentifier();
            TokenType type = Keywords.TryGetValue(word, out TokenType keywordType)
                ? keywordType
                : TokenType.Identifier;

            return new Token(type, word, startLine, startColumn);
        }

        if (char.IsDigit(ch))
        {
            string numberStr = ReadNumber();

            if (!long.TryParse(numberStr, out long value) || value < int.MinValue || value > int.MaxValue)
            {
                _reader.AddError($"Число '{numberStr}' выходит за пределы допустимого диапазона Integer.");
            }

            return new Token(TokenType.IntegerLiteral, numberStr, startLine, startColumn);
        }

        switch (ch)
        {
            case ';':
                _reader.NextCh();
                return new Token(TokenType.Semicolon, ";", startLine, startColumn);
            case ':':
                _reader.NextCh();
                if (_reader.CurrentChar == '=')
                {
                    _reader.NextCh();
                    return new Token(TokenType.Assign, ":=", startLine, startColumn);
                }
                return new Token(TokenType.Colon, ":", startLine, startColumn);
            case ',':
                _reader.NextCh();
                return new Token(TokenType.Comma, ",", startLine, startColumn);
            case '.':
                _reader.NextCh();
                return new Token(TokenType.Dot, ".", startLine, startColumn);
            case '=':
                _reader.NextCh();
                return new Token(TokenType.Equal, "=", startLine, startColumn);
            case '+':
                _reader.NextCh();
                return new Token(TokenType.Plus, "+", startLine, startColumn);
            case '-':
                _reader.NextCh();
                return new Token(TokenType.Minus, "-", startLine, startColumn);
            case '*':
                _reader.NextCh();
                return new Token(TokenType.Star, "*", startLine, startColumn);
            case '/':
                _reader.NextCh();
                return new Token(TokenType.Slash, "/", startLine, startColumn);
            case '(':
                _reader.NextCh();
                return new Token(TokenType.OpenParen, "(", startLine, startColumn);
            case ')':
                _reader.NextCh();
                return new Token(TokenType.CloseParen, ")", startLine, startColumn);
            default:
                _reader.AddError($"Неизвестный символ '{ch}'");
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
                while (!_reader.IsEof && _reader.CurrentChar != '}')
                {
                    _reader.NextCh();
                }

                if (!_reader.IsEof)
                {
                    _reader.NextCh();
                }
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
}