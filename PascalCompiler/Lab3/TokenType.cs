namespace Lab3;

public enum TokenType
{
    Eof = 0,
    Unknown = 1,

    Program = 2,
    Var = 3,
    Const = 4,
    Begin = 5,
    End = 6,
    If = 7,
    Then = 8,
    Else = 9,
    Case = 10,
    Of = 11,
    For = 12,
    To = 13,
    Downto = 14,
    Do = 15,

    IntegerType = 16,
    BooleanType = 17,

    Identifier = 20,
    IntegerLiteral = 21,
    BooleanLiteral = 22,

    Semicolon = 30,
    Colon = 31,
    Comma = 32,
    Dot = 33,
    Assign = 34,
    Equal = 35,
    Plus = 36,
    Minus = 37,
    Star = 38,
    Slash = 39,
    OpenParen = 40,
    CloseParen = 41
}