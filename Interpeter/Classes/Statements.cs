
public interface StatementVisitor<T>
{
    T VisitExpressionStatement(ExpressionStatement expressionStatement);
    T VisitWrite(Write write);
    T VisitVar(Var var);
}
public abstract class Statements
{
    
    public abstract T Accept<T>(StatementVisitor<T> visitor);

}

public class Write : Statements
{
    Expression expression;
    public Expression Expression { get => expression; set => expression = value; }
    public Write(Expression expression)
    {
        this.expression = expression;
    }
    public override T Accept<T>(StatementVisitor<T> visitor)
    {
        return visitor.VisitWrite(this);
    }
}

public class ExpressionStatement : Statements
{
    Expression expression;
    public Expression Expression { get => expression; set => expression = value; }

    public ExpressionStatement(Expression expression)
    {
        this.expression = expression;
    }
     public override T Accept<T>(StatementVisitor<T> visitor){
       return visitor.VisitExpressionStatement(this);
    }
}
public class Var:Statements {
    private Token name;
    private Expression initializer;

    public Token Name { get => name; set => name = value; }
    public Expression Initializer {get => initializer; set => initializer = value;}
    public Var(Token name, Expression initializer){
        this.name = name;
        this.initializer = initializer;
    }
    public override T Accept<T>(StatementVisitor<T> visitor)
    {
        return visitor.VisitVar(this);
    }
}