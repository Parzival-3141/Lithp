using System;
using System.Collections.Generic;

namespace Lithp
{
    public enum TokenType
    {
        // The values of these three match their equivalents in ExprType
        SYMBOL = 0, 
        NUMBER = 1,
        STRING = 2,

        L_PAREN,
        R_PAREN,
        QUOTE,

        EOF,
    }

    public struct Token
    {
        public TokenType type;
        public object value;

        public Token(TokenType type, object value)
        {
            this.type = type;
            this.value = value;
        }
    }

    public static class Lexer
    {
        static int index;
        static string text = "";

        public static List<Token> Lex(string text)
        {
            var tokens = new List<Token>();
            Lexer.text = text;
            index = 0;

            while (!AtEof())
            {
                char current = EatChar();
                switch (current)
                {
                    case '(': tokens.AddTkn(TokenType.L_PAREN); break;
                    case ')': tokens.AddTkn(TokenType.R_PAREN); break;

                    case ';': while(!AtEof() && EatChar() != '\n') { /* nop */ } break;

                    case '-':
                        {
                            if (char.IsDigit(Peek()))
                                tokens.Add(LexNumber(current));
                            else
                                //tokens.AddTkn((TokenType)'-');
                                tokens.Add(LexSymbol(current));
                        } break;

                    case '"': tokens.Add(LexString()); break;
                    case '\'': tokens.AddTkn(TokenType.QUOTE); break;

                    default:
                        if (char.IsDigit(current))
                        {
                            tokens.Add(LexNumber(current));
                        }
                        else if (IsValidSymbolChar(current))
                        {
                            tokens.Add(LexSymbol(current));
                        }
                        else if (char.IsWhiteSpace(current))
                        {
                            break;
                        }
                        else
                        {
                            Program.Exit($"Error: Unsupported character '{current}'");
                        }

                        break;
                }
            }

            tokens.AddTkn(TokenType.EOF);
            return tokens;
        }

        private static bool AtEof()
        {
            return index >= text.Length;
        }

        private static bool IsValidSymbolChar(char c)
        {
            return c >= 33 && c <= 126
                // exclude exclusive token chars
                && c != '\'' 
                && c != '(' && c != ')'
                && c != ';'
                && c != '"';
        }

        private static Token LexSymbol(char first)
        {
            string name = "" + first;
            while(!AtEof() && IsValidSymbolChar(Peek()))
            {
                name += EatChar();
            }

            return new Token(TokenType.SYMBOL, name);
        }

        private static Token LexNumber(char first)
        {
            string number = "" + first;
            bool hasDecimal = false;

            while (!AtEof())
            {
                char c = Peek();

                if (!char.IsDigit(c))
                {
                    if (c == '.')
                    {
                        if (!hasDecimal)
                        {
                            hasDecimal = true;
                            number += EatChar();
                            if (char.IsDigit(Peek()))
                            {
                                number += EatChar();
                            }
                            else
                            {
                                Program.Exit("Error: floating point numbers must contain a digit after the decimal");
                            }
                        }
                        else
                        {
                            Program.Exit("Error: numbers can have only one decimal");
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    number += EatChar();
                }
            }

            //var tkn = new Token(TokenType.NUMBER, null);
            
            //if (hasDecimal)
            //    tkn.value = float.Parse(number);
            //else
            //    tkn.value = int.Parse(number);
            //return tkn;
            
            // @Note: every number is a double, because it's simpler that way.
            return new Token(TokenType.NUMBER, double.Parse(number));
        }

        private static Token LexString()
        {
            string str = "";
            bool escaped = false;
            bool foundClosingQuote = false;

            while(!AtEof())
            {
                if(Peek() == '\\')
                {
                    escaped = true;
                    str += EatChar();
                    continue;
                }

                if(Peek() == '"' && !escaped)
                {
                    foundClosingQuote = true;
                    _ = EatChar(); // get rid of closing quotation
                    break;
                }
                else
                {
                    str += EatChar();
                }

                escaped = false;
            }


            if (!foundClosingQuote)
            {
                Program.Exit("Error: Unclosed string!");
            }

            return new Token(TokenType.STRING, str);
        }

        private static char EatChar()
        {
            return text[index++];
        }

        private static char Peek()
        {
            return text[index];
        }

        private static void AddTkn(this List<Token> tokens, TokenType type, object value = null)
        {
            tokens.Add(new Token(type, value));
        }
    }
}
