

using System.Runtime.InteropServices;

public class Environment
{
    private Dictionary<string, object> values = new Dictionary<string, object>();

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
        throw new RunTimeError(name, "Undefined variable");
    }

    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }
        throw new RunTimeError(name, "undefined variable");
    }
}