
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class Interpeter : Visitor<object>
{


    public object VisitLiteralExpression(Literal literalExpression)
    {
        return literalExpression.Value;
    }


    public object VisitGroupingExpression(Grouping groupingExpression)
    {
        return Evaluate(groupingExpression.Expression);
    }

    public object VisitUnaryExpression(Unary unaryExpression)
    {
        object right = Evaluate(unaryExpression.RightExpression);

        switch (unaryExpression.Operation.TokenType)
        {
            case TokenType.MINUS:
                return -(double)right;
            case TokenType.NOT:
                return !IsTrue(right);
        }
        return null;
    }
    public object VisitBinaryExperssion(BinaryExpression binaryExperssion)
    {

        object left = Evaluate(binaryExperssion.Left);
        object right = Evaluate(binaryExperssion.Right);

        switch (binaryExperssion.Operation.TokenType)
        {
            case TokenType.MINUS:
                return (double)left - (double)right;
            case TokenType.PLUS:
                if (left is string && right is string)
                {
                    return (string)left + (string)right;
                }
                if (left is int && right is int || right is double && left is double)
                {
                    return (double)left + (double)right;
                }
                break;
            case TokenType.SLASH:
                return (double)left / (double)right;
            case TokenType.STAR:
                return (double)left * (double)right;
            case TokenType.GREATER:
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                return (double)left >= (double)right;
            case TokenType.LESS:
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                return (double)left <= (double)right;
            case TokenType.NOT_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
        }
        return null;
    }


    private object Evaluate(Expression expression)
    {
        return expression.Accept(this);
    }
    private bool IsTrue(object expression)
    {
        if (expression == null)
            return false;
        if (expression is bool)
            return (bool)expression;
        return true;
    }

    private bool IsEqual(object left, object right)
    {
        if (left == null && right == null)
            return true;
        if (left == null)
            return false;
        return left.Equals(right);
    }

    public void Interpert(Expression expression)
    {
        object value = Evaluate(expression);
        System.Console.WriteLine(ToString(value));
    }

    private string ToString(object value)
    {
        if (value == null)
            return "nul";
        if (value is double)
        {
            string text = value.ToString();
            if (text.EndsWith(".0"))
                text = text.Substring(0, text.Length - 2);
                 return text;
        }
        return value.ToString();
    }
}