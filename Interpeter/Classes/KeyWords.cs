public class KeyWords
{
    public static Dictionary<string, TokenType> keywords = new Dictionary<string, TokenType>
{
    { "and", TokenType.AND },
    { "class", TokenType.CLASS },
    { "else", TokenType.ELSE },
    { "false", TokenType.FALSE },
    { "for", TokenType.FOR },
    { "function", TokenType.METHOD },
    { "if", TokenType.IF },
    { "nil", TokenType.NUL },
    { "or", TokenType.OR },
    { "print", TokenType.WRITE },
    { "return", TokenType.RETURN },
    { "super", TokenType.SUPER },
    { "this", TokenType.THIS },
    { "true", TokenType.TRUE },
    { "var", TokenType.VAR },
    { "while", TokenType.WHILE },
    {"int", TokenType.NUMBER },
    {"Write", TokenType.WRITE },
    {"break",TokenType.BREAK},
};
}