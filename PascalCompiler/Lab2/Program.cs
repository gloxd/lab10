using Lab2;

Console.WriteLine("=== ЗАДАНИЕ 2: Синтаксический анализатор (Parser) ===\n");

// Тестовый код с синтаксическими ошибками:
// 1. Пропущена двоеточие в объявлении 'x integer;'
// 2. Использован неизвестный оператор в присваивании
// 3. Забыта точка в самом конце
string testCode = @"program SyntaxDemo;
var
    x integer; 
    y: integer;
begin
    x = 10;
    y := x + ;
end";

Console.WriteLine("--- Исходный код ---");
Console.WriteLine(testCode);
Console.WriteLine("--------------------\n");

var reader = new InputReader(testCode);
var lexer = new Lexer(reader);
var parser = new Parser(lexer, reader);

parser.ParseProgram();

Console.WriteLine("Результаты синтаксического анализа:");
if (reader.Errors.Count > 0)
{
    foreach (var err in reader.Errors)
    {
        Console.WriteLine($"  -> {err}");
    }
}
else
{
    Console.WriteLine("  Синтаксических ошибок не обнаружено!");
}