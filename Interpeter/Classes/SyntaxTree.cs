using System.Reflection.Metadata.Ecma335;
using System.Runtime.Versioning;

public interface Visitor<T>
{
    T VisitBinaryExperssion(BinaryExpression binaryExpression);
    T VisitGroupingExpression(Grouping grouping);
    T VisitLiteralExpression(Literal literal);
    T VisitUnaryExpression(Unary unary);
    T VisitVariable(Variable variable);
    T VisitAssign(Assign assign);
    T VisitLogical(Logical logical);
    T VisitCall(Call call);
}

public abstract class Expression
{
    public abstract T Accept<T>(Visitor<T> visitor);
}

public class Assign : Expression
{
    private Token name;
    private Expression value;

    public Token Name { get => name; set => name = value; }
    public Expression Value { get => value; set => this.value = value; }
    public Assign(Token name, Expression value)
    {
        this.name = name;
        this.value = value;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitAssign(this);
    }
}
public class BinaryExpression : Expression
{
    private Expression left;
    private Token operation;
    private Expression right;

    public Expression Left { get => left; set => left = value; }
    public Token Operation { get => operation; set => operation = value; }
    public Expression Right { get => right; set => right = value; }

    public BinaryExpression(Expression left, Token operation, Expression right)
    {
        this.left = left;
        this.operation = operation;
        this.right = right;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitBinaryExperssion(this);
    }
}

public class Grouping : Expression
{
    private Expression expression;
    public Expression Expression { get => expression; set => expression = value; }

    public Expression InnerExpression { get => expression; set => expression = value; }

    public Grouping(Expression expression)
    {
        this.expression = expression;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitGroupingExpression(this);
    }
}

public class Literal : Expression
{
    private object value;

    public object Value { get => value; set => this.value = value; }

    public Literal(object value)
    {
        this.value = value;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitLiteralExpression(this);
    }
}

public class Unary : Expression
{
    private Token operation;
    private Expression rightExpression;

    public Token Operation { get => operation; set => operation = value; }
    public Expression RightExpression { get => rightExpression; set => rightExpression = value; }

    public Unary(Token operation, Expression rightExpression)
    {
        this.operation = operation;
        this.rightExpression = rightExpression;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitUnaryExpression(this);
    }
}

public class Variable : Expression
{
    private Token name;
    public Token Name { get => name; set => name = value; }
    public Variable(Token name)
    {
        this.name = name;
    }
    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitVariable(this);
    }
}

public class Logical : Expression
{
    private Expression left;

    private Token operation;
    private Expression right;
    public Expression Left { get => left; set => left = value; }
    public Token Operation { get => operation; set => operation = value; }
    public Expression Right { get => right; set => right = value; }
    public Logical(Expression left, Token operation, Expression right)
    {
        this.left = left;
        this.operation = operation;
        this.right = right;
    }
    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitLogical(this);
    }
}
public interface PsagotCallable
{

    public int Arity();
    public object Call(Interpeter interpeter, List<object> arguments);
}
public class PsagotFunction : PsagotCallable
{
    private Function declaration;
    private Environment closure;

    public PsagotFunction(Function declaration, Environment closure)
    {
        this.closure = closure;
        this.declaration = declaration;
    }
    public int Arity() => declaration.Parameters.Count;
    public object Call(Interpeter interpeter, List<object> arguments)
    {
        Environment environment = new Environment(closure);
        for (int i = 0; i < declaration.Parameters.Count; i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, arguments[i]);
        }
        try
        {
            interpeter.ExecuteBlock(declaration.Body, environment);

        }
        catch(ReturnException returnException){
            return returnException.Value;
        }
        return null;
    }

    public override string ToString()
    {
        return $"<fn {declaration.Name.Lexeme}>";
    }
}
public class ClockFunction : PsagotCallable
{
    public int Arity() => 0;
    public object Call(Interpeter interpeter, List<object> arguments)
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
    }
    public override string ToString()
    {
        return "<Native function>";
    }
}
public class Call : Expression
{
    private Expression callee;
    private Token paren;
    private List<Expression> arguements;
    public Expression Callee { get => callee; set => callee = value; }
    public Token Paren { get => paren; set => paren = value; }
    public List<Expression> Arguments { get => arguements; set => arguements = value; }

    public Call(Expression callee, Token paren, List<Expression> arguements)
    {
        this.callee = callee;
        this.paren = paren;
        this.arguements = arguements;
    }
    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitCall(this);
    }
}

