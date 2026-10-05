using Lab1;

Console.WriteLine("=== ЗАДАНИЕ 1: Лексический анализатор с расширенной диагностикой ===\n");

string testCode = @"program LexerDemo;
var
    12abc: integer; { Ошибка: имя начинается с цифр }
    very_long_idеntifier_name_that_exceeds_the_maximum_allowed_length_limit_in_pascal: integer; { Предупреждение: длина }
const
    c = 99999999999999; { Ошибка: переполнение integer }
begin
    d := 10 @; { Ошибка: неизвестный символ @ }
    { Ошибка: не запущен закрывающий комментарий
end.";

Console.WriteLine("--- Исходный код ---");
Console.WriteLine(testCode);
Console.WriteLine("--------------------\n");

var reader = new InputReader(testCode);
var lexer = new Lexer(reader);
var tokens = new List<Token>();
Token token;

do
{
    token = lexer.NextToken();
    if (token.Type != TokenType.Eof)
    {
        tokens.Add(token);
    }
} while (token.Type != TokenType.Eof);

Console.WriteLine("Обнаруженные лексические ошибки и предупреждения:");
if (reader.Errors.Count > 0)
{
    foreach (var err in reader.Errors)
    {
        Console.WriteLine($"  -> {err}");
    }
}
else
{
    Console.WriteLine("  Ошибок не обнаружено.");
}