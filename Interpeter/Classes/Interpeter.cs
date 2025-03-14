
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Xml;

public class Interpeter : Visitor<object>, StatementVisitor<object>
{

    private Environment environment = new Environment();
    public object VisitExpressionStatement(ExpressionStatement expressionStatement)
    {
        Evaluate(expressionStatement.Expression);
        return null;
    }

    public object VisitWrite(Write writeStatement)
    {
        object value = Evaluate(writeStatement.Expression);
        Console.WriteLine(ToString(value));
        return null;
    }
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
                IsNumberOperand(unaryExpression.Operation, right);
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
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left - (double)right;
            case TokenType.PLUS:
                if ((left is string && right is string) || (left is string || right is string))
                {
                    return left.ToString() + right.ToString();
                }
                if (left is int && right is int || right is double && left is double)
                {
                    return (double)left + (double)right;
                }
                throw new RunTimeError(binaryExperssion.Operation, "Addition expects two string or two numbers");
            case TokenType.SLASH:
                if ((double)right == 0 || (int)right == 0)
                    throw new RunTimeError(binaryExperssion.Operation, "Attempt of division by 0");
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left * (double)right;
            case TokenType.GREATER:
                if (left is string && right is string)
                {
                    string left2 = (string)left;
                    string right2 = (string)right;
                    return left2.CompareTo(right2) > 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                if (left is string && right is string)
                {
                    string left2 = (string)left;
                    string right2 = (string)right;
                    return left2.CompareTo(right2) >= 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                if (left is string && right is string)
                {
                    string left2 = (string)left;
                    string right2 = (string)right;
                    return left2.CompareTo(right2) < 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                if (left is string && right is string)
                {
                    string left2 = (string)left;
                    string right2 = (string)right;
                    return left2.CompareTo(right2) <= 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left <= (double)right;
            case TokenType.NOT_EQUAL:
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return IsEqual(left, right);
        }
        return null;
    }

    public object VisitVar(Var var)
    {
        object value = null;
        if (var.Initializer != null)
        {
            value = Evaluate(var.Initializer);
        }
        environment.Define(var.Name.Lexeme, value);
        return null;
    }
    public object VisitVariable(Variable variableExpression)
    {
        return environment.GetValue(variableExpression.Name);
    }

    public object VisitAssign(Assign assignExpression)
    {

        object value = Evaluate(assignExpression.Value);
        environment.Assign(assignExpression.Name, value);
        return value;
    }
    public object VisitBlock(Block block)
    {
        ExecuteBlock(block.Statements, new Environment(environment));
        return null;
    }

    public void ExecuteBlock(List<Statements> statements, Environment environment)
    {
        Environment previous = this.environment;
        try
        {
            this.environment = environment;
            foreach (Statements statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    public object VisitIf(If ifStatement)
    {
        if (IsTrue(Evaluate(ifStatement.Condition)))
            Execute(ifStatement.ThenBranch);
        else
        {
            if (ifStatement.ElseBranch != null)
                Execute(ifStatement.ElseBranch);
        }
        return null;
    }
    public object VisitLogical(Logical logical)
    {
        object left = Evaluate(logical.Left);
        if (logical.Operation.TokenType == TokenType.OR)
        {
            if (IsTrue(left))
            {
                return left;
            }
        }
        else
        {
            if (!IsTrue(left))
                return left;
        }
        return Evaluate(logical.Right);
    }

    public object VisitWhileLoop(WhileLoop loop){
        while(IsTrue(Evaluate(loop.Condition)))
            Execute(loop.Body);
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

    public void Interpert(List<Statements> program)
    {
        try
        {
            foreach (Statements statement in program)
            {
                Execute(statement);
            }
        }
        catch (RunTimeError runTimeError)
        {
            Psagot.RunTimeError(runTimeError);
        }
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
    private void Execute(Statements statement)
    {
        statement.Accept(this);
    }
    public void IsNumberOperand(Token operation, object operand)
    {
        if (operand is double || operand is int)
            return;
        throw new RunTimeError(operation, "Expected a number");
    }
    public void IsNumberOperand(Token operation, object left, object right)
    {
        if ((left is double && right is double) || (right is int && left is int))
            return;
        throw new RunTimeError(operation, "Expected a number");
    }

}



public class RunTimeError : Exception
{
    private Token token;
    private string message;

    public Token Token { get => token; set => token = value; }
    public string Message { get => message; set => message = value; }
    public RunTimeError(Token operation, string message)
    {
        this.token = operation;
        this.message = message;
    }

}