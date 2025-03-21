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
    { "return", TokenType.RETURN },
    { "super", TokenType.SUPER },
    { "this", TokenType.THIS },
    { "true", TokenType.TRUE },
    { "var", TokenType.VAR },
    { "while", TokenType.WHILE },
    {"int", TokenType.NUMBER },
    {"write", TokenType.WRITE },
    {"break",TokenType.BREAK},
    {"=>",TokenType.LAMBDA},
};
}