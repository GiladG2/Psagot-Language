
using System;
using System.IO;

public class Psagot
{
    static bool hadError = false;
    static bool hadRuntimeError = false;
    private readonly static Interpeter interpeter = new Interpeter();
    public static void Main(string[] args)
    {
        if (args.Length > 1)
        {
            throw new Exception("Usage : Sky Interpeter");
        }
        else
        if (args.Length == 1)
        {
            runFile(args[0]);
        }
        else
        {
            runPrompt();
        }
    }

    //Way 1 - 
    private static void runFile(string path)
    {
        string source = System.IO.File.ReadAllText(path);
        run(source);
    }

    //Way 2 - Interactive
    private static void runPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            string line = Console.ReadLine();
            if (line == null) break;
            run(line);
            hadError = false;
            hadRuntimeError = false;
        }
    }
    private static void run(string source)
    {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.scanTokens();
        Parser parser = new Parser(tokens);
        List<Statements> statements = parser.Parse();

        foreach (Token token in tokens)
        {
            if (hadError)
                return;
            if (hadRuntimeError)
                return;
        }
        Resolver resolver = new Resolver(interpeter);
        resolver.Resolve(statements);
        if (hadError)
            return;
        interpeter.Interpert(statements);

    }

    public static void error(int line, string message)
    {
        report(line, "", message);
    }

    public static void error(Token token, string message)
    {
        if (token.TokenType == TokenType.EOF)
        {
            report(token.Line, " at end", message);
        }
        else
        {
            report(token.Line, " at '" + token.Lexeme + "'", message);
        }
    }
    public static void report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line} ] An error occured {where} : {message}");
        hadError = true;
    }

    public static void RunTimeError(RunTimeError runTimeError)
    {
        System.Console.WriteLine(runTimeError.Message + "\n[line " + runTimeError.Token.Line + "]");
        hadRuntimeError = true;
    }
}
