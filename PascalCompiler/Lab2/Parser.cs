namespace Lab2;

public class Parser
{
    private readonly Lexer _lexer;
    private readonly InputReader _reader;
    private Token _currentToken = null!;

    public Parser(Lexer lexer, InputReader reader)
    {
        _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        Advance(); // Загружаем первый токен
    }

    private void Advance()
    {
        _currentToken = _lexer.NextToken();
    }

    // Вспомогательный метод: проверяет и считывает ожидаемый токен, иначе фиксирует ошибку
    private bool Expect(TokenType expectedType, string message, ErrorCode code = ErrorCode.ExpectedToken)
    {
        if (_currentToken.Type == expectedType)
        {
            Advance();
            return true;
        }

        _reader.AddError($"[Синтаксис] {message}. Получено: '{_currentToken.Value}'", code);
        return false;
    }

    // Разбор всей программы: program Name; var ... begin ... end.
    public void ParseProgram()
    {
        // 1. Заголовок программы
        if (_currentToken.Type == TokenType.Program)
        {
            Advance();
            Expect(TokenType.Identifier, "Ожидалось имя программы после 'program'", ErrorCode.InvalidProgramHeader);
            Expect(TokenType.Semicolon, "Ожидалась ';' после имени программы");
        }
        else
        {
            _reader.AddError("Программа должна начинаться с ключевого слова 'program'", ErrorCode.InvalidProgramHeader);
        }

        // 2. Блок объявлений (var)
        if (_currentToken.Type == TokenType.Var)
        {
            ParseVarBlock();
        }

        // 3. Основной блок (begin ... end)
        if (_currentToken.Type == TokenType.Begin)
        {
            ParseCompoundStatement();
        }
        else
        {
            _reader.AddError("Ожидалось ключевое слово 'begin' для начала исполняемого блока", ErrorCode.UnbalancedBeginEnd);
        }

        // 4. Точка в конце программы
        Expect(TokenType.Dot, "Ожидалась точка '.' в конце программы");
    }

    private void ParseVarBlock()
    {
        Expect(TokenType.Var, "Ожидалось 'var'");

        while (_currentToken.Type == TokenType.Identifier)
        {
            Advance(); // Идентификатор переменной

            // Поддержка нескольких переменных через запятую (x, y: integer;)
            while (_currentToken.Type == TokenType.Comma)
            {
                Advance();
                Expect(TokenType.Identifier, "Ожидалось имя переменной после ','", ErrorCode.InvalidDeclaration);
            }

            Expect(TokenType.Colon, "Ожидалось двоеточие ':' после списка переменных");

            if (_currentToken.Type == TokenType.Identifier)
            {
                string typeName = _currentToken.Value.ToLower();
                if (typeName is not ("integer" or "string" or "real" or "boolean"))
                {
                    _reader.AddError($"Неизвестный тип данных '{_currentToken.Value}'", ErrorCode.InvalidDeclaration, ErrorSeverity.Warning);
                }
                Advance();
            }
            else
            {
                _reader.AddError("Ожидался тип данных (integer, string и т.д.)", ErrorCode.InvalidDeclaration);
            }

            Expect(TokenType.Semicolon, "Ожидалась ';' после объявления переменной");
        }
    }

    private void ParseCompoundStatement()
    {
        Expect(TokenType.Begin, "Ожидалось 'begin'");

        while (_currentToken.Type != TokenType.End && _currentToken.Type != TokenType.Eof)
        {
            ParseStatement();

            if (_currentToken.Type == TokenType.Semicolon)
            {
                Advance();
            }
            else if (_currentToken.Type != TokenType.End)
            {
                _reader.AddError("Ожидалась ';' между операторами");
                Synchronize(); // Восстановление после ошибки
            }
        }

        Expect(TokenType.End, "Ожидался 'end' для закрытия блока 'begin'", ErrorCode.UnbalancedBeginEnd);
    }

    private void ParseStatement()
    {
        if (_currentToken.Type == TokenType.Identifier)
        {
            Advance(); // Имя переменной
            if (_currentToken.Type == TokenType.Assign)
            {
                Advance(); // :=
                ParseExpression();
            }
            else
            {
                _reader.AddError("Ожидался оператор присваивания ':='", ErrorCode.InvalidExpression);
            }
        }
        else if (_currentToken.Type == TokenType.Begin)
        {
            ParseCompoundStatement();
        }
        else
        {
            _reader.AddError($"Недопустимое начало оператора '{_currentToken.Value}'", ErrorCode.InvalidExpression);
            Advance();
        }
    }

    private void ParseExpression()
    {
        // Простой разбор операнда (число или переменная)
        if (_currentToken.Type is TokenType.Identifier or TokenType.IntegerLiteral)
        {
            Advance();
        }
        else
        {
            _reader.AddError($"Некорректное выражение около '{_currentToken.Value}'", ErrorCode.InvalidExpression);
            return;
        }

        // Если идет бинарный оператор (+, -, *, /)
        if (_currentToken.Type is TokenType.Plus or TokenType.Minus or TokenType.Star or TokenType.Slash)
        {
            Advance();
            ParseExpression(); // Рекурсивно разбираем правую часть
        }
    }

    // Режим "паники" для пропуска токенов до точки с запятой в случае ошибки
    private void Synchronize()
    {
        while (_currentToken.Type != TokenType.Eof)
        {
            if (_currentToken.Type == TokenType.Semicolon)
            {
                Advance();
                return;
            }
            if (_currentToken.Type is TokenType.Begin or TokenType.End or TokenType.Var)
            {
                return;
            }
            Advance();
        }
    }
}