using System.Reflection.Metadata.Ecma335;

public interface Visitor<T>
{
    T VisitBinaryExperssion(BinaryExpression binaryExpression);
    T VisitGroupingExpression(Grouping grouping);
    T VisitLiteralExpression(Literal literal);
    T VisitUnaryExpression(Unary unary);
    T VisitVariable(Variable variable);
    T VisitAssign(Assign assign);
    T VisitLogical(Logical logical);
}

public abstract class Expression
{
    public abstract T Accept<T>(Visitor<T> visitor);
}

public class Assign:Expression{
    private Token name;
    private Expression value;
    
    public Token Name {get => name; set => name = value; }
    public Expression Value {get => value; set => this.value = value;}
    public Assign(Token name, Expression value){
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
    public Expression Expression  { get => expression; set => expression = value; }

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

public class Variable :Expression{
    private Token name;
    public Token Name { get => name; set => name = value; }
    public Variable(Token name) {
        this.name = name;
    }
    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitVariable(this);
    }
}

public class Logical:Expression{
    private Expression left;

    private Token operation;
    private Expression right;
     public Expression Left { get => left; set => left = value; }
     public Token Operation {get => operation; set => operation = value; }
     public Expression Right { get => right; set => right = value; }
    public Logical(Expression left, Token operation, Expression right){
        this.left = left;
        this.operation = operation;
        this.right = right;
    }
    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitLogical(this);
    }
}