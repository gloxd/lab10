namespace Lab3;

public class Parser
{
    private readonly Lexer _lexer;
    private readonly InputReader _reader;
    private Token _currentToken;
    private readonly Dictionary<string, TokenType> _symbolTable = new(StringComparer.OrdinalIgnoreCase);

    public Parser(Lexer lexer, InputReader reader)
    {
        _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _currentToken = _lexer.NextToken();
    }

    public void ParseProgram()
    {
        if (_currentToken.Type == TokenType.Program)
        {
            NextToken();
            Expect(TokenType.Identifier, "Ожидалось имя программы");
            Expect(TokenType.Semicolon, "Ожидалась ';' после имени программы");
        }

        if (_currentToken.Type == TokenType.Var)
        {
            ParseVarDeclarations();
        }

        ParseCompoundStatement();

        if (_currentToken.Type == TokenType.Dot)
        {
            NextToken();
        }
        else
        {
            AddError("Программа должна завершаться точкой '.'");
        }
    }

    private void ParseVarDeclarations()
    {
        Expect(TokenType.Var, "Ожидалось ключевое слово 'var'");

        while (_currentToken.Type == TokenType.Identifier)
        {
            var identifiers = new List<Token>();

            do
            {
                if (_currentToken.Type == TokenType.Identifier)
                {
                    identifiers.Add(_currentToken);
                    NextToken();
                }

                if (_currentToken.Type == TokenType.Comma)
                {
                    NextToken();
                }
                else
                {
                    break;
                }
            } while (true);

            Expect(TokenType.Colon, "Ожидалось ':' после списка идентификаторов");

            TokenType varType = TokenType.Unknown;
            if (_currentToken.Type is TokenType.IntegerType or TokenType.BooleanType)
            {
                varType = _currentToken.Type;
                NextToken();
            }
            else
            {
                AddError("Ожидался простой тип данных (integer или boolean)");
                Synchronize(TokenType.Semicolon);
            }

            Expect(TokenType.Semicolon, "Ожидалась ';' после типа");

            foreach (var id in identifiers)
            {
                if (!_symbolTable.TryAdd(id.Value, varType))
                {
                    AddError($"Семантическая ошибка: Переменная '{id.Value}' уже была объявлена ранее.", ErrorCode.RedeclaredIdentifier);
                }
            }
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
                NextToken();
            }
            else if (_currentToken.Type != TokenType.End)
            {
                AddError("Ожидалась ';' между операторами");
                Synchronize(TokenType.Semicolon, TokenType.End);
            }
        }

        Expect(TokenType.End, "Ожидалось 'end'");
    }

    private void ParseStatement()
    {
        switch (_currentToken.Type)
        {
            case TokenType.Begin:
                ParseCompoundStatement();
                break;
            case TokenType.If:
                ParseIfStatement();
                break;
            case TokenType.Case:
                ParseCaseStatement();
                break;
            case TokenType.For:
                ParseForStatement();
                break;
            case TokenType.Identifier:
                ParseAssignmentStatement();
                break;
            default:
                break;
        }
    }

    private void ParseAssignmentStatement()
    {
        string varName = _currentToken.Value;
        Expect(TokenType.Identifier, "Ожидался идентификатор");

        if (!_symbolTable.ContainsKey(varName))
        {
            AddError($"Семантическая ошибка: Переменная '{varName}' не объявлена.", ErrorCode.UndeclaredIdentifier);
        }

        Expect(TokenType.Assign, "Ожидался оператор ':='");

        TokenType exprType = ParseExpression();

        if (_symbolTable.TryGetValue(varName, out TokenType varType) && varType != exprType && exprType != TokenType.Unknown)
        {
            AddError($"Семантическая ошибка: Несоответствие типов! Нельзя присвоить {exprType} переменной {varType}.", ErrorCode.TypeMismatch);
        }
    }

    private void ParseIfStatement()
    {
        Expect(TokenType.If, "Ожидалось 'if'");
        ParseExpression();
        Expect(TokenType.Then, "Ожидалось 'then'");
        ParseStatement();

        if (_currentToken.Type == TokenType.Else)
        {
            NextToken();
            ParseStatement();
        }
    }

    private void ParseCaseStatement()
    {
        Expect(TokenType.Case, "Ожидалось 'case'");
        ParseExpression();
        Expect(TokenType.Of, "Ожидалось 'of'");

        while (_currentToken.Type is TokenType.IntegerLiteral or TokenType.BooleanLiteral or TokenType.Identifier)
        {
            NextToken();
            Expect(TokenType.Colon, "Ожидалось ':' после константы");
            ParseStatement();

            if (_currentToken.Type == TokenType.Semicolon)
            {
                NextToken();
            }
        }

        Expect(TokenType.End, "Ожидалось 'end' для case");
    }

    private void ParseForStatement()
    {
        Expect(TokenType.For, "Ожидалось 'for'");

        string loopVar = _currentToken.Value;
        Expect(TokenType.Identifier, "Ожидался счетчик цикла");

        if (!_symbolTable.ContainsKey(loopVar))
        {
            AddError($"Семантическая ошибка: Счетчик цикла '{loopVar}' не объявлен.", ErrorCode.UndeclaredIdentifier);
        }

        Expect(TokenType.Assign, "Ожидался оператор ':='");
        ParseExpression();

        if (_currentToken.Type is TokenType.To or TokenType.Downto)
        {
            NextToken();
        }
        else
        {
            AddError("Ожидалось 'to' или 'downto'");
        }

        ParseExpression();
        Expect(TokenType.Do, "Ожидалось 'do'");
        ParseStatement();
    }

    private TokenType ParseExpression()
    {
        TokenType leftType = ParseTerm();

        while (_currentToken.Type is TokenType.Plus or TokenType.Minus)
        {
            NextToken();
            TokenType rightType = ParseTerm();

            if (leftType == TokenType.IntegerType && rightType == TokenType.IntegerType)
            {
                leftType = TokenType.IntegerType;
            }
        }

        return leftType;
    }

    private TokenType ParseTerm()
    {
        TokenType leftType = ParseFactor();

        while (_currentToken.Type is TokenType.Star or TokenType.Slash)
        {
            NextToken();
            TokenType rightType = ParseFactor();

            if (leftType == TokenType.IntegerType && rightType == TokenType.IntegerType)
            {
                leftType = TokenType.IntegerType;
            }
        }

        return leftType;
    }

    private TokenType ParseFactor()
    {
        if (_currentToken.Type == TokenType.IntegerLiteral)
        {
            NextToken();
            return TokenType.IntegerType;
        }

        if (_currentToken.Type == TokenType.BooleanLiteral)
        {
            NextToken();
            return TokenType.BooleanType;
        }

        if (_currentToken.Type == TokenType.Identifier)
        {
            string name = _currentToken.Value;
            NextToken();

            if (_symbolTable.TryGetValue(name, out TokenType type))
            {
                return type;
            }

            AddError($"Семантическая ошибка: Переменная '{name}' не объявлена.", ErrorCode.UndeclaredIdentifier);
            return TokenType.Unknown;
        }

        if (_currentToken.Type == TokenType.OpenParen)
        {
            NextToken();
            TokenType exprType = ParseExpression();
            Expect(TokenType.CloseParen, "Ожидалась закрывающая скобка ')'");
            return exprType;
        }

        AddError($"Неожиданный токен '{_currentToken.Value}'");
        return TokenType.Unknown;
    }

    private void Synchronize(params TokenType[] syncTokens)
    {
        while (_currentToken.Type != TokenType.Eof)
        {
            foreach (var token in syncTokens)
            {
                if (_currentToken.Type == token)
                {
                    return;
                }
            }
            NextToken();
        }
    }

    private void Expect(TokenType expectedType, string errorMessage)
    {
        if (_currentToken.Type == expectedType)
        {
            NextToken();
        }
        else
        {
            AddError(errorMessage);
            Synchronize(TokenType.Semicolon, TokenType.End);
        }
    }

    private void NextToken()
    {
        _currentToken = _lexer.NextToken();
    }

    private void AddError(string message, ErrorCode code = ErrorCode.UndeclaredIdentifier)
    {
        _reader.AddError(message, code);
    }
}