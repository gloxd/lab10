using Lab3;

Console.WriteLine("=== ЗАДАНИЕ 3: Синтаксический и семантический анализатор (Вариант 6) ===\n");

string testCode = @"program Variant6Demo;
var
    i, total: integer;
    flag: boolean;
begin
    total := 10 + 2 * (5 - 1);
    flag := true;

    { Семантическая ошибка 1: Необъявленная переменная }
    x := 5; 
    y := 4;

    if flag then
    begin
        for i := 1 to total do
            total := total - 1;
    end;

    case total of
        1: flag := false;
        2: flag := true;
    end;
end.";

Console.WriteLine("Исходная программа:");
Console.WriteLine(testCode);
Console.WriteLine("--------------------------------------------------\n");

var reader = new InputReader(testCode);
var lexer = new Lexer(reader);
var parser = new Parser(lexer, reader);

parser.ParseProgram();

if (reader.Errors.Count == 0)
{
    Console.WriteLine(">>> Программа успешно прошла синтаксический и семантический анализ! <<<");
}
else
{
    Console.WriteLine($">>> Найдено ошибок: {reader.Errors.Count} <<<");
    foreach (var err in reader.Errors)
{
    Console.WriteLine($"  -> {err}");
}
}