using System;
using System.Collections.Generic;

public class Environment
{
    private readonly Dictionary<string, object> values = new Dictionary<string, object>();
    public readonly Environment enclosing;

    public Environment(Environment enclosing = null)
    {
        this.enclosing = enclosing;
    }

    public void Define(string name, object value)
    {
        values[name] = value;
    }

    public object Get(Token name)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            return values[name.Lexeme];
        }
        if (enclosing != null) return enclosing.Get(name);
        Psagot.error(name, "Undefined variable '" + name.Lexeme + "'.");
        return null;
    }

    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }
        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }
        Psagot.error(name, "Undefined variable '" + name.Lexeme + "'.");
    }

    public object GetAt(int distance, string name)
    {
        return Ancestor(distance).values[name];
    }

    public void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance).values[name.Lexeme] = value;
    }

    private Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.enclosing;
        }
        return environment;
    }
}
