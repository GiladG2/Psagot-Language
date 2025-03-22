using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Versioning;

public class Resolver : Visitor<object>, StatementVisitor<object>
{
    private readonly Interpeter interpreter;
    private readonly Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
    private FunctionType currentFunction = FunctionType.NONE;

    private enum FunctionType { NONE, FUNCTION }

    public Resolver(Interpeter interpreter)
    {
        this.interpreter = interpreter;
    }

    // Statements

    public object VisitBlock(Block block)
    {
        BeginScope();
        Resolve(block.Statements);
        EndScope();
        return null;
    }

    public object VisitVar(Var varStmt)
    {
        Declare(varStmt.Name);
        if (varStmt.Initializer != null)
        {
            Resolve(varStmt.Initializer);
        }
        Define(varStmt.Name);
        return null;
    }

    public object VisitExpressionStatement(ExpressionStatement stmt)
    {
        Resolve(stmt.Expression);
        return null;
    }

    public object VisitWrite(Write writeStmt)
    {
        Resolve(writeStmt.Expression);
        return null;
    }

    public object VisitIf(If ifStmt)
    {
        Resolve(ifStmt.Condition);
        Resolve(ifStmt.ThenBranch);
        if (ifStmt.ElseBranch != null)
            Resolve(ifStmt.ElseBranch);
        return null;
    }

    public object VisitWhileLoop(WhileLoop whileLoop)
    {
        Resolve(whileLoop.Condition);
        Resolve(whileLoop.Body);
        return null;
    }

    public object VisitBreak(Break breakStmt)
    {
        // Nothing to resolve.
        return null;
    }

    public object VisitFunction(Function function)
    {
        Declare(function.Name);
        Define(function.Name);
        ResolveFunction(function, FunctionType.FUNCTION);
        return null;
    }

    public object VisitReturn(Return returnStmt)
    {
        if (currentFunction == FunctionType.NONE)
        {
            throw new RunTimeError(returnStmt.Keyword, "Cannot return from top-level code.");
        }
        if (returnStmt.Value != null)
        {
            Resolve(returnStmt.Value);
        }
        return null;
    }

    // Expressions

    public object VisitBinaryExperssion(BinaryExpression expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object VisitGroupingExpression(Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object VisitLiteralExpression(Literal expr)
    {
        return null;
    }

    public object VisitUnaryExpression(Unary expr)
    {
        Resolve(expr.RightExpression);
        return null;
    }

    public object VisitVariable(Variable expr)
    {
        if (scopes.Count > 0 &&
            scopes.Peek().ContainsKey(expr.Name.Lexeme) &&
            scopes.Peek()[expr.Name.Lexeme] == false)
        {
            Psagot.error(expr.Name, "Cannot read local variable in its own initializer.");
        }
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object VisitAssign(Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object VisitLogical(Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object VisitCall(Call expr)
    {
        Resolve(expr.Callee);
        foreach (Expression argument in expr.Arguments)
        {
            Resolve(argument);
        }
        return null;
    }

    public object VisitLambda(LambdaExpression lambdaExpr)
    {
        BeginScope();
        foreach (Token param in lambdaExpr.Parameters)
        {
            Declare(param);
            Define(param);
        }
        // Assume lambda body is a list of statements.
        Resolve(lambdaExpr.Body);
        EndScope();
        return null;
    }

    // Helper methods

    private void Resolve(Statements stmt)
    {
        stmt?.Accept(this);
    }

    private void Resolve(Expression expr)
    {
        expr?.Accept(this);
    }

    public void Resolve(List<Statements> statements)
    {
        foreach (Statements stmt in statements)
        {
            Resolve(stmt);
        }
    }

    private void ResolveFunction(Function function, FunctionType type)
    {
        FunctionType enclosingFunction = currentFunction;
        currentFunction = type;
        BeginScope();
        foreach (Token param in function.Parameters)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.Body);
        EndScope();
        currentFunction = enclosingFunction;
    }

    private void ResolveLocal(Expression expr, Token name)
    {
        int i = 0;
        foreach (var scope in scopes)
        {
            if (scope.ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, i);
                return;
            }
            i++;
        }
    }

    private void BeginScope()
    {
        scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (scopes.Count == 0) return;
        var scope = scopes.Peek();
        if (scope.ContainsKey(name.Lexeme))
        {
            Psagot.error(name, "Already a variable with this name in this scope.");
            throw new RunTimeError(name, "Already a variable with this name in this scope.");
        }
        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (scopes.Count == 0) return;
        scopes.Peek()[name.Lexeme] = true;
    }
}
