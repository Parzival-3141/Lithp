using System;
using System.Collections.Generic;

namespace Lithp
{
    public enum ExprType
    {
        // The values of these three match their equivalents in TokenType
        SYMBOL = 0,
        NUMBER = 1,
        STRING = 2,
        LIST,
    }

    public struct Expr
    {
        public ExprType type;
        public object content;

        public Expr(ExprType type, object content)
        {
            this.type = type;
            this.content = content;
        }
    }

    public static class Parser
    {
        static int index;
        static Token[] tokens;
        static List<Expr> expressions;

        public static List<Expr> Parse(Token[] tokens)
        {
            index = 0;
            Parser.tokens = tokens;
            expressions = new List<Expr>();

            if(tokens.Length == 0)
            {
                Program.Exit("No tokens to parse");
            }

            while (!AtEof())
            {
                expressions.Add(ParseExpr());
            }

            return expressions;
        }

        private static Expr ParseExpr()
        {
            var expr = new Expr();
            var tkn = EatToken();
            switch (tkn.type)
            {
                // @Note: We can fallthrough like this because these three values are
                // castable. (See TokenType and ExprType)
                case TokenType.SYMBOL:
                case TokenType.NUMBER:
                case TokenType.STRING:
                    expr.type = (ExprType)tkn.type;
                    expr.content = tkn.value;
                    break;
                
                case TokenType.L_PAREN:
                    expr.type = ExprType.LIST;
                    expr.content = new List<Expr>();

                    while(!Match(TokenType.R_PAREN))
                    {
                        ((List<Expr>)expr.content).Add(ParseExpr());
                    }

                    Expect(TokenType.R_PAREN);
                    break;

                case TokenType.QUOTE:
                    expr.type = ExprType.LIST;
                    expr.content = new List<Expr>() 
                    { 
                        new Expr(ExprType.SYMBOL, "quote"),
                        ParseExpr()
                    };
                    break;

                case TokenType.EOF:
                    Program.Exit("Error: Parser encountered EOF, expected an expression");
                    break;
            }

            return expr;
        }

        // Assumes list.Count > 0
        private static T Pop<T>(this List<T> list)
        {
            int last = list.Count - 1;

            T value = list[last];
            list.RemoveAt(last);

            return value;
        }

        private static bool AtEof()
        {
            return Match(TokenType.EOF) || index >= tokens.Length;
        }

        private static Token EatToken()
        {
            return tokens[index++];
        }

        private static Token Peek()
        {
            return tokens[index];
        }

        private static bool Match(TokenType type)
        {
            return Peek().type == type;
        }

        private static Token Expect(TokenType type)
        {
            var t = Peek();
            if (t.type != type)
            {
                Program.Exit($"Error: Expected Token '{type}' at index '{index}', got '{t.type}'");
            }

            return EatToken();
        }
    }
}
