namespace Lab1;

public record Token(
    TokenType Type,
    string Value,
    int Line,
    int Column
);