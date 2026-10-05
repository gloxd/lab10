using Lab0;

Console.WriteLine("=== ЗАДАНИЕ 0: Модуль ввода-вывода с расширенной диагностикой ===\n");

// Входная строка с тестом кириллицы, непечатного символа \a и незакрытой кавычки
string testCode = "program Test;\nvar\n\tx: intеger; \a\n  str := 'незакрытая строка";

Console.WriteLine("Исходный текст:");
Console.WriteLine(testCode);
Console.WriteLine("\n-----------------------------------------------\n");

var reader = new InputReader(testCode);

while (!reader.IsEof)
{
    reader.NextCh();
}

Console.WriteLine("Таблица обнаруженных ошибок:");
if (reader.Errors.Count == 0)
{
    Console.WriteLine("Ошибок не обнаружено.");
}
else
{
    foreach (var err in reader.Errors)
    {
        Console.WriteLine($"  -> {err}");
    }
}