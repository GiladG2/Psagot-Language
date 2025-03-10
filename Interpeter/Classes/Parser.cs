using System.Text.RegularExpressions;

public class Parser
{
    private List<Token> tokens;
    private int current = 0;
    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }
    public Parser()
    {
    }
    private Expression ExpressionParse()
    {
        return Assignment();
    }

    private Expression Assignment(){
        Expression expression = Equality();

        if(Match([TokenType.EQUAL])){
            Token equals = previous();
            Expression value = Assignment(); 

            if(expression is Variable){
                Token name = ((Variable)expression).Name;
                return new Assign(name,value);
            }

            Error(equals,"Invalid assignment target");
        }
        return expression;

    }
    private Expression Equality()
    {
        Expression expression = Comparison();


        while (Match([TokenType.NOT_EQUAL, TokenType.EQUAL_EQUAL]))
        {
            Token operation = previous();
            Expression rightExpression = Comparison();
            expression = new BinaryExpression(expression, operation, rightExpression);
        }
        return expression;

    }

    private bool Match(TokenType[] tokenTypes)
    {
        foreach (TokenType tokenType in tokenTypes)
        {
            if (Check(tokenType))
            {
                advance();
                return true;
            }
        }
        return false;
    }
    private bool Check(TokenType tokenType)
    {
        if (isAtEnd()) return false;
        return peek().TokenType == tokenType;
    }
    public Token advance()
    {
        if (!isAtEnd()) current++;
        return previous();
    }
    public bool isAtEnd()
    {
        return peek().TokenType == TokenType.EOF;
    }
    public Token peek()
    {
        return current < tokens.Count ? tokens[current] : null;
    }
    public Token previous()
    {
        return tokens[current - 1];
    }

    private Expression Comparison()
    {
        Expression expression = Term();

        while (Match([TokenType.GREATER_EQUAL, TokenType.GREATER, TokenType.LESS, TokenType.LESS_EQUAL]))
        {
            Token operation = previous();
            Expression rightExpression = Term();
            expression = new BinaryExpression(expression, operation, rightExpression);
        }
        return expression;
    }

    private Expression Term()
    {
        Expression expression = Factor();

        while (Match([TokenType.MINUS, TokenType.PLUS]))
        {
            Token operation = previous();
            Expression rightExpression = Factor();
            expression = new BinaryExpression(expression, operation, rightExpression);
        }
        return expression;
    }

    private Expression Factor()
    {
        Expression expression = Unary();

        while (Match([TokenType.STAR, TokenType.SLASH]))
        {
            Token operation = previous();
            Expression rightExpression = Unary();
            expression = new BinaryExpression(expression, operation, rightExpression);
        }
        return expression;
    }

    private Expression Unary()
    {

        if (Match([TokenType.NOT, TokenType.MINUS]))
        {
            Token operation = previous();
            Expression rightExpression = Unary();
            return new Unary(operation, rightExpression);
        }
        return Primary();
    }

    private Expression Primary()
    {
        if (Match([TokenType.FALSE]))
        {
            return new Literal(false);
        }
        if (Match([TokenType.TRUE]))
        {
            return new Literal(true);
        }
        if (Match([TokenType.NUL]))
        {
            return new Literal(null);
        }
        if (Match([TokenType.NUMBER, TokenType.STRING]))
        {
            return new Literal(previous().Literal);
        }
        if (Match([TokenType.IDENTIFIER]))
            return new Variable(previous());
        if (Match([TokenType.LEFT_PAREN]))
        {
            Expression expression = ExpressionParse();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after an expression");
            return new Grouping(expression);
        }
        throw Error(peek(), "Expect expression.");
    }

    private Token Consume(TokenType tokenType, string message)
    {
        if (Check(tokenType))
            return advance();
        throw Error(peek(), message);
    }

    private ParseError Error(Token token, string message)
    {
        Psagot.error(token, message);
        return new ParseError();
    }


    public List<Statements> Parse()
    {
        List<Statements> statements = new List<Statements>();
        while (!isAtEnd())
            statements.Add(Declaration());
        return statements;

    }

    private Statements Declaration()
    {
        try
        {
            if (Match([TokenType.VAR]))
                return VarDeclaration();
            return Statement();
        }
        catch (ParseError parseError)
        {
            parseError.Synchronize();
            return null;
        }
    }
    private Statements VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expects variable name");
        Expression init = null;
        if (Match([TokenType.EQUAL]))
            init = ExpressionParse();
        return new Var(name, init);
    }
    private Statements Statement()
    {
        if (Match([TokenType.WRITE]))
            return WriteStatement();
        return ExpressionStatement();
    }

    private Statements WriteStatement()
    {
        Expression value = ExpressionParse();
        return new Write(value);
    }

    private Statements ExpressionStatement()
    {
        Expression value = ExpressionParse();
        return new ExpressionStatement(value);
    }
}

public class ParseError : Exception
{
    private readonly Parser parser = new Parser();
    public void Synchronize()
    {
        parser.advance();

        while (parser.isAtEnd())
        {
            if (parser.previous().TokenType == TokenType.SEMICOLON)
                return;
            switch (parser.peek().TokenType)
            {
                case TokenType.CLASS:
                case TokenType.METHOD:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.WRITE:
                case TokenType.RETURN:
                    return;
            }
            parser.advance();
        }
    }

}