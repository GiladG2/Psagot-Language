
using System.Diagnostics;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Xml;

using System;
using System.Collections.Generic;
using System.Linq;
public class Interpeter : Visitor<object>, StatementVisitor<object>
{
    public static Environment globals = new Environment();
    private Environment environment = globals;
    // Resolver integration: map expressions to their scope depth.
    private readonly Dictionary<Expression, int> locals = new Dictionary<Expression, int>();

    public Environment Globals { get => globals; set => globals = value; }

    public Interpeter()
    {
        globals.Define("clock", new ClockFunction());
    }

    // Resolver will call this method.
    public void Resolve(Expression expr, int depth)
    {
        locals[expr] = depth;
    }

    // Looks up a variable, using resolved scope depth if available.
    private object LookUpVariable(Token name, Expression expr)
    {
        if (locals.ContainsKey(expr))
        {
            int distance = locals[expr];
            return environment.GetAt(distance, name.Lexeme);
        }
        return globals.Get(name);
    }

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
                if ((left is int && right is int) || (left is double && right is double))
                {
                    return (double)left + (double)right;
                }
                throw new RunTimeError(binaryExperssion.Operation, "Addition expects two strings or two numbers");
            case TokenType.SLASH:
                if (((double)right) == 0 || ((int)right) == 0)
                    throw new RunTimeError(binaryExperssion.Operation, "Division by zero");
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left * (double)right;
            case TokenType.GREATER:
                if (left is string && right is string)
                {
                    return ((string)left).CompareTo((string)right) > 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                if (left is string && right is string)
                {
                    return ((string)left).CompareTo((string)right) >= 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                if (left is string && right is string)
                {
                    return ((string)left).CompareTo((string)right) < 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                if (left is string && right is string)
                {
                    return ((string)left).CompareTo((string)right) <= 0;
                }
                IsNumberOperand(binaryExperssion.Operation, left, right);
                return (double)left <= (double)right;
            case TokenType.NOT_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
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
        return LookUpVariable(variableExpression.Name, variableExpression);
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
                if (statement is Break)
                {
                    break;
                }
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
        else if (ifStatement.ElseBranch != null)
            Execute(ifStatement.ElseBranch);
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

    public object VisitWhileLoop(WhileLoop loop)
    {
        while (IsTrue(Evaluate(loop.Condition)))
        {
            Execute(loop.Body);
        }
        return null;
    }

    public object VisitBreak(Break breakStatement)
    {
        // For simplicity, break just returns false (or handle it as needed).
        return false;
    }

    public object VisitCall(Call call)
    {
        object calleeVis = Evaluate(call.Callee);
        List<object> arguments = new List<object>();
        foreach (Expression expression in call.Arguments)
        {
            arguments.Add(Evaluate(expression));
        }
        PsagotCallable function;
        if (!(calleeVis is PsagotCallable))
            throw new RunTimeError(call.Paren, "Can only call functions and classes");
        else
            function = (PsagotCallable)calleeVis;
        if (arguments.Count != function.Arity())
            throw new RunTimeError(call.Paren, $"Expected {function.Arity()} arguments, but got {arguments.Count}.");
        return function.Call(this, arguments);
    }

    public object VisitFunction(Function function)
    {
        PsagotFunction psagotFunction = new PsagotFunction(function, environment);
        environment.Define(function.Name.Lexeme, psagotFunction);
        return null;
    }

    public object VisitLambda(LambdaExpression lambda)
    {
        return new PsagotLambdaFunction(lambda, new Environment(environment));
    }

    public object VisitReturn(Return returnStatement)
    {
        object value = null;
        if (returnStatement.Value != null)
        {
            value = Evaluate(returnStatement.Value);
        }
        throw new ReturnException(value);
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
        if ((left is double && right is double) || (left is int && right is int))
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