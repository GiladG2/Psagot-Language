using System.Runtime.InteropServices;
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

    private Expression Assignment()
    {
        Expression expression = Or();

        if (Match([TokenType.EQUAL]))
        {
            Token equals = previous();
            Expression value = Assignment();

            if (expression is Variable)
            {
                Token name = ((Variable)expression).Name;
                return new Assign(name, value);
            }
            Error(equals, "Invalid assignment target");
        }
        return expression;

    }

    private Expression Or()
    {
        Expression expression = And();
        while (Match([TokenType.OR]))
        {
            Token operation = previous();
            Expression rightExpression = And();
            expression = new Logical(expression, operation, rightExpression);
        }
        return expression;
    }
    private Expression And()
    {
        Expression expression = Equality();
        while (Match([TokenType.AND]))
        {
            Token operation = previous();
            Expression rightExpression = Equality();
            expression = new Logical(expression, operation, rightExpression);
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
        return Callee();
    }
    public Expression Callee(){
        Expression expression = Primary();

        while(true){
            if(Match([TokenType.LEFT_PAREN]))
              expression = FinishCall(expression);
            else
               break;
        }

        return expression;
    }

    private Expression FinishCall(Expression expression){
         List<Expression> arguments = new List<Expression>();
         if(!Check(TokenType.RIGHT_PAREN)){
            do{
                if(arguments.Count >=255)
                 Error(peek(),"A function cannot hold more than 255 arguments");
                arguments.Add(ExpressionParse());
                
            }
            while(Match([TokenType.COMMA]));
         }
         Token paren = Consume(TokenType.RIGHT_PAREN,"Expects ')' after arguments");
         return new Call(expression, paren, arguments);
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
        Consume(TokenType.SEMICOLON, "Expects a ';' after var declaration");
        return new Var(name, init);
    }
    private Statements Statement()
    {
            if (Match([TokenType.WRITE]))
                return WriteStatement();
            if (Match([TokenType.LEFT_BRACE]))
                return new Block(BlockStatement());
            if (Match([TokenType.IF]))
                return IfStatement();
            if (Match([TokenType.WHILE]))
                return WhileStatement();
            if (Match([TokenType.FOR]))
                return ForStatement();
            if (Match([TokenType.BREAK]))
                return BreakStatement();
            if(Match([TokenType.METHOD]))
               return FunctionStatement("function");
            if(Match([TokenType.RETURN]))
               return ReturnStatement();
            return ExpressionStatement();       
    }

    private Statements ReturnStatement(){
        Token keyword = previous();
        Expression value = null;
        if(!Check(TokenType.SEMICOLON))
           value = ExpressionParse();
        Consume(TokenType.SEMICOLON,"Expects ';' after return");
        return new Return(keyword,value);
    }

    private Statements FunctionStatement(string kind){
        Token name = Consume(TokenType.IDENTIFIER,$"Expects {kind} name.");
        Consume(TokenType.LEFT_PAREN,$"Expects '( after {kind} name.");
        List<Token> parameters = new List<Token>();
        if(!Check(TokenType.RIGHT_PAREN)){
            do{
                if(parameters.Count >= 255){
                    Error(peek(),"Can't have more than 255 arguments.");
                }
                parameters.Add(Consume(TokenType.IDENTIFIER,"Expects parameter name."));
            }
            while(Match([TokenType.COMMA]));
        }
        
        Consume(TokenType.RIGHT_PAREN,"Expects ')' after parameters.");
        Consume(TokenType.LEFT_BRACE,"Expects '{'' before + " +  $"{kind} body.");
        List<Statements> body = BlockStatement();
        return new Function(name,parameters,body);
    }
    private Statements BreakStatement()
    {
        Consume(TokenType.SEMICOLON, "Expectes ';' after break");
        return new Break();
    }
    private Statements ForStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expects '(' after 'for'");

        Statements initializer;
        if (Match([TokenType.SEMICOLON]))
            initializer = null;
        else
        {
            if (Match([TokenType.VAR]))
                initializer = VarDeclaration();
            else
                initializer = ExpressionStatement();
        }
        Expression condition = null;
        if (!Check(TokenType.SEMICOLON))
            condition = ExpressionParse();
        Consume(TokenType.SEMICOLON, "Expects ';' after loop condition");
        Expression increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
            increment = ExpressionParse();
        Consume(TokenType.RIGHT_PAREN, "Expects ')' after for clauses");
        Statements body = Statement();

        if (increment != null)
        {
            body = new Block(new List<Statements> { body, new ExpressionStatement(increment) });
        }

        if (condition == null)
        {
            condition = new Literal(true);
        }

        body = new WhileLoop(condition, body);

        if (initializer != null)
        {
            body = new Block(new List<Statements> { initializer, body });
        }

        return body;
    }

    private Statements WhileStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expectes '(' after  while");
        Expression condition = ExpressionParse();
        Consume(TokenType.RIGHT_PAREN, "Expectes ')' after while condition");
        Statements body = Statement();
        return new WhileLoop(condition, body);
    }
    private Statements IfStatement()
    {
        Consume(TokenType.LEFT_PAREN, "Expectes '(' after an if");
        Expression condition = ExpressionParse();
        Consume(TokenType.RIGHT_PAREN, "Expectes ')' after if condition");
        Statements thenBranch = Statement();
        Statements elseBranch = null;
        if (Match([TokenType.ELSE]))
            elseBranch = Statement();
        return new If(condition, thenBranch, elseBranch);

    }
    private Statements WriteStatement()
    {
        Expression value = ExpressionParse();
        Consume(TokenType.SEMICOLON, "Expects a ';' after Write");
        return new Write(value);
    }

    private Statements ExpressionStatement()
    {
        Expression value = ExpressionParse();
        Consume(TokenType.SEMICOLON, "Expects a ';' after expression");

        return new ExpressionStatement(value);
    }

    private List<Statements> BlockStatement()
    {
        List<Statements> statements = new List<Statements>();

        while (!Check(TokenType.RIGHT_BRACE) && !isAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expects '}' at the end of a block");
        return statements;
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