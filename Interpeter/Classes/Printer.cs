using System.Text;

public class AstPrinter : Visitor<string>
{
    public string Print(Expression expression)
    {
        return expression.Accept(this);
    }
    public string VisitBinaryExperssion(BinaryExpression binaryExpression)
    {
        return Parenthesize(binaryExpression.Operation.Lexeme, new Expression[] { binaryExpression.Left, binaryExpression.Right });
    }
    public string VisitGroupingExpression(Grouping groupingExpression)
    {
        return Parenthesize("group", new Expression[] { groupingExpression.Expression });
    }
    public string VisitLiteralExpression(Literal literalExpression)
    {
        if (literalExpression.Value == null)
            return "nil";
        return literalExpression.Value.ToString();
    }

    public string VisitUnaryExpression(Unary unaryExpression)
    {
        return Parenthesize(unaryExpression.Operation.Lexeme, new Expression[] { unaryExpression.RightExpression });

    }
    public string VisitVariable(Variable variable)
    {
        return Parenthesize(variable.Name.Lexeme, new Expression[2]);
    }
    public string VisitAssign(Assign assign)
    {
        return Parenthesize(assign.Name.Lexeme, [assign.Value]);
    }
    public string VisitLogical(Logical logical)
    {

        return Parenthesize(logical.Operation.Lexeme, [logical.Left, logical.Right]);
    }

    public string VisitLambda(LambdaExpression lambda)
    {

        return Parenthesize(lambda.ToString(), [lambda]);
    }
    public string VisitCall(Call call)
    {

        return Parenthesize(call.Paren.Literal.ToString(), [call.Callee]);
    }
    private string Parenthesize(string name, Expression[] expression)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("(").Append(name);
        foreach (Expression expr in expression)
        {
            stringBuilder.Append(" ");
            stringBuilder.Append(expr.Accept(this));
        }
        stringBuilder.Append(")");
        return stringBuilder.ToString();
    }
}