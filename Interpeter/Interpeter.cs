
using System;


public class Psagot
{
    public static void Main(string[] args)
    {
        if(args.Length > 1){
            throw new Exception("Usage : Sky Interpeter");
            //Environment.Exit(64);
        }
        else
        if(args.Length == 1){
            //runFile(args[0]);
        }
        else{
            //runPrompt();
        }
    }
}