using System;
using System.Collections.Generic;
using System.Globalization;

public class Scanner
{
    private string source; // source code
    private List<Token> tokens = new List<Token>();
    private int start = 0;
    private int current = 0;
    private int line = 1;

    public Scanner(string source)
    {
        this.source = source;
    }

    public List<Token> scanTokens()
    {
        while (current < source.Length)
        {
            start = current;
            scanToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, line)); // End of file
        return tokens;
    }

    private void scanToken()
    {
        char c = advance();
        switch (c)
        {
            case '(': addToken(TokenType.LEFT_PAREN); break;
            case ')': addToken(TokenType.RIGHT_PAREN); break;
            case '{': addToken(TokenType.LEFT_BRACE); break;
            case '}': addToken(TokenType.RIGHT_BRACE); break;
            case ',': addToken(TokenType.COMMA); break;
            case '.': addToken(TokenType.DOT); break;
            case '-': addToken(TokenType.MINUS); break;
            case '+': addToken(TokenType.PLUS); break;
            case ';': addToken(TokenType.SEMICOLON); break;
            case '*':
                if (peek() == '/')
                    Psagot.error(line, "Comment has no opening tag");
                else
                    addToken(TokenType.STAR);
                break;
            case '!': addToken(match('=') ? TokenType.NOT_EQUAL : TokenType.NOT); break;
            case '<': addToken(match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': addToken(match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            case '=': addToken(match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '/':
                if (peek() == '*')
                {
                    advance();
                    advance();
                    while (peek() != '*' && current < source.Length) advance();
                    if (current >= source.Length)
                    {
                        Psagot.error(line, "Unclosed comment");
                        break;
                    }
                    advance();
                    if (peek() == '/')
                    {
                        advance();
                        break;
                    }
                    else
                    {
                        Psagot.error(line, "Incorrect comment syntax");
                        break;
                    }
                }
                if (peek() == '/')
                {
                    while (peek() != '\n' && current < source.Length) advance();
                }

                else
                {
                    addToken(TokenType.SLASH);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                line++;
                break;
            case '"': String(); break;

            default:
                if (isDigit(c))
                {
                    Int();
                }
                else if (isAlpha(c))
                {
                    identifier();
                }
                else
                {
                    Psagot.error(line, "Unexpected character.");
                }
                break;
        }
    }

    private bool isDigit(char c)
    {
        return c >= '0' && c <= '9';
    }

    private bool isAlpha(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               c == '_';
    }

    private bool isAlphaNumeric(char c)
    {
        return isAlpha(c) || isDigit(c);
    }

    private void identifier()
    {
        while (isAlphaNumeric(peek())) advance();
        string text = source.Substring(start, current - start);
        TokenType tokenType = KeyWords.keywords.ContainsKey(text) ? KeyWords.keywords[text] : TokenType.IDENTIFIER;
        addToken(tokenType);
    }

    private void Int()
    {
        while (isDigit(peek()) && current < source.Length) advance();

        // Check for decimal numbers
        if (peek() == '.' && isDigit(peekNext()))
        {
            advance(); // Consume '.'
            while (isDigit(peek()) && current < source.Length) advance();
        }

        string numberText = source.Substring(start, current - start);
        double integer = double.Parse(numberText, CultureInfo.InvariantCulture);
        addToken(TokenType.NUMBER, integer);
    }

    private void String()
    {
        while (peek() != '"' && current < source.Length)
        {
            if (peek() == '\n') line++;
            advance();
        }

        if (current >= source.Length)
        {
            Psagot.error(line, "Undefined string.");
            return;
        }

        advance(); // Closing quote
        string text = source.Substring(start + 1, (current - start - 2)); // Remove surrounding quotes
        addToken(TokenType.STRING, text);
    }

    // Lookahead
    private char peek()
    {
        if (current >= source.Length) return '\0';
        return source[current];
    }

    private char peekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }

    private char advance()
    {
        return source[current++];
    }

    private bool match(char expected)
    {
        if (current >= source.Length || source[current] != expected) return false;
        current++;
        return true;
    }

    private void addToken(TokenType type)
    {
        addToken(type, null);
    }

    private void addToken(TokenType type, object literal)
    {
        string text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }
}
