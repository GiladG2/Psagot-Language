using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;


public enum TokenType
{

    // Single-character tokens.
    LEFT_PAREN, RIGHT_PAREN, LEFT_BRACE, RIGHT_BRACE,
    COMMA, DOT, MINUS, PLUS, SEMICOLON, SLASH, STAR,

    // One or two character tokens.
    NOT, NOT_EQUAL,
    EQUAL, EQUAL_EQUAL,
    GREATER, GREATER_EQUAL,
    LESS, LESS_EQUAL,

    // Literals.
    IDENTIFIER, STRING, NUMBER,

    // Keywords.
    AND, CLASS, ELSE, FALSE, METHOD, FOR, IF, NUL, OR,
    WRITE, RETURN, SUPER, THIS, TRUE, VAR, WHILE,BREAK,LAMBDA,

    EOF //End of file
}


public class Token
{
    private TokenType tokenType;
    private string lexeme;
    private object literal;
    private int line;
     

    public string Lexeme {get => lexeme; set => lexeme = value; }
    public TokenType TokenType {get => tokenType; set=> tokenType = value;} 
    public object Literal {get => literal; set => literal = value; }
    public int Line{get => line; set => line = value;}
    public Token(TokenType tokenType, string lexeme, object literal, int line)
    {
        this.tokenType = tokenType;
        this.lexeme = lexeme;
        this.literal = literal;
        this.line = line;
    }

    public override string ToString()
    {
        return $"Token {line}, {lexeme}, {literal}";
    }
}


