
public interface StatementVisitor<T>
{
    T VisitExpressionStatement(ExpressionStatement expressionStatement);
    T VisitWrite(Write write);
    T VisitVar(Var var);
    T VisitBlock(Block block);
    T VisitIf(If ifStatement);
    T VisitWhileLoop(WhileLoop whileLoop);
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
    public override T Accept<T>(StatementVisitor<T> visitor)
    {
        return visitor.VisitExpressionStatement(this);
    }
}
public class Var : Statements
{
    private Token name;
    private Expression initializer;

    public Token Name { get => name; set => name = value; }
    public Expression Initializer { get => initializer; set => initializer = value; }
    public Var(Token name, Expression initializer)
    {
        this.name = name;
        this.initializer = initializer;
    }
    public override T Accept<T>(StatementVisitor<T> visitor)
    {
        return visitor.VisitVar(this);
    }
}

public class Block : Statements
{

    List<Statements> statements;
    public List<Statements> Statements { get => statements; set => statements = value; }
    public Block(List<Statements> statements)
    {
        this.statements = statements;
    }
    public override T Accept<T>(StatementVisitor<T> visitor)
    {
        return visitor.VisitBlock(this);
    }
}

public class If : Statements
{
    private Expression condition;
    private Statements thenBranch;
    private Statements elseBranch;
    public Expression Condition { get => condition; set => condition = value; }
    public Statements ThenBranch { get => thenBranch; set => thenBranch = value; }
    public Statements ElseBranch { get => elseBranch; set => elseBranch = value; }
    public If(Expression condition, Statements thenBranch, Statements elseBranch)
    {
        this.condition = condition;
        this.thenBranch = thenBranch;
        this.elseBranch = elseBranch;
    }
    public override T Accept<T>(StatementVisitor<T> visitor)
    {
        return visitor.VisitIf(this);
    }
}

public class WhileLoop : Statements
{
    private Expression condition;
    private Statements body;

    public Expression Condition { get => condition; set => condition = value; }
    public Statements Body { get => body; set => body = value; }
    public WhileLoop(Expression condition, Statements body)
    {
        this.condition = condition;
        this.body = body;
    }

    public override T Accept<T>(StatementVisitor<T> visitor)
    {
        return visitor.VisitWhileLoop(this);
    }
}