

using System.Runtime.InteropServices;

public class Environment
{

    private Environment enclosing;
    private Dictionary<string, object> values = new Dictionary<string, object>();

    public Environment()
    {
        this.enclosing = null;
    }
    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }
    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public object GetValue(Token name)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            return values[name.Lexeme];
        }
        if (enclosing != null)
        {
            return enclosing.GetValue(name);
        }
        throw new RunTimeError(name, "Undefined variable");
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
              enclosing.Assign(name,value);
              return;
        }
        throw new RunTimeError(name, "undefined variable");
    }
}