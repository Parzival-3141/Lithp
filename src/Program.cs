using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lithp
{
    static class Program
    {
        public static void Exit(string msg)
        {
            Console.WriteLine(msg);
            Environment.Exit(1);
        }

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Repl();
                return;
            }

            if(args.Contains("-h") || args.Contains("-help"))
            {
                Console.WriteLine("lithp [-h | -help] [filepath]\nPassing no arguments starts the REPL");
                return;
            }
                
            string text = "";
            using (var reader = File.OpenText(args[0]))
            {
                text = reader.ReadToEnd();
            }

            var tokens = Lexer.Lex(text);
            Console.WriteLine("Lexer Output:");
            Console.WriteLine(PrettyPrint(tokens));

            Console.WriteLine("\nParser Output:");
            var expressions = Parser.Parse(tokens.ToArray());
            //DebugPrint(expressions);
            Console.WriteLine(PrettyPrint(expressions));
        }

        public static void Repl()
        {
            Console.WriteLine("Lithp REPL (Type 'exit' to close)");
            while (true)
            {
                Console.Write(">>");
                string txt = Console.ReadLine();
                if (txt == "exit")
                    break;

                var exprs = Parser.Parse(Lexer.Lex(txt).ToArray());
                //@Todo: evaluate
                Console.WriteLine("AST:\n" + PrettyPrint(exprs));
            }
        }

        public static string PrettyPrint(List<Token> tokens)
        {
            string result = "";
            int depth = 0;

            void TryAddWhiteSpace(int index)
            {
                if (index + 1 < tokens.Count && tokens[index + 1].type != TokenType.R_PAREN)
                {
                    if(depth > 0)
                        result += ' ';
                    else
                        result += '\n';
                }
            }

            for (int i = 0; i < tokens.Count; i++)
            {

                Token t = tokens[i];
                switch (t.type)
                {
                    case TokenType.SYMBOL:
                    case TokenType.NUMBER:
                        result += t.value;
                        TryAddWhiteSpace(i);
                        break;

                    case TokenType.STRING:
                        result += '"' + (string)t.value + '"';
                        TryAddWhiteSpace(i);
                        break;

                    case TokenType.QUOTE:   result += '\'';  break;
                    case TokenType.L_PAREN: result += '('; depth++; break;
                    case TokenType.R_PAREN: result += ')'; depth--; TryAddWhiteSpace(i); break;
                }
            }

            return result;
        }

        public static string PrettyPrint(List<Expr> exprs, int depth = 0)
        {
            string result = "";
            
            void TryAddWhiteSpace(int index)
            {
                if (depth <= 0)
                {
                    result += "\n";
                }
                else if(index < exprs.Count - 1)
                {
                    result += " ";
                }
            }

            for (int i = 0; i < exprs.Count; i++)
            {
                Expr e = exprs[i];
                switch (e.type)
                {
                    case ExprType.SYMBOL:
                    case ExprType.NUMBER:
                        result += e.content.ToString();
                        break;

                    case ExprType.STRING:
                        result += "\"" + e.content.ToString() + "\"";
                        break;

                    case ExprType.LIST:
                        result += "(" + PrettyPrint((List<Expr>)e.content, depth + 1) + ")";
                        break;
                }
                
                TryAddWhiteSpace(i);
            }

            return result;
        }

        public static void DebugPrint(List<Expr> exprs, int depth = 0)
        {
            string AddIndent(int depth)
            {
                string indent = "";
                for (int i = 1; i <= depth; i++)
                {
                    indent += "    ";
                }
                return indent;
            }

            foreach(var e in exprs)
            {
                if(e.type == ExprType.LIST)
                {
                    Console.WriteLine(AddIndent(depth) + "LIST (");
                    DebugPrint((List<Expr>)e.content, depth + 1);   
                    Console.WriteLine(AddIndent(depth) + ")");
                }
                else
                {
                    Console.WriteLine(AddIndent(depth) + e.type + " " + e.content.ToString());
                }
            }
        }
    }
}
