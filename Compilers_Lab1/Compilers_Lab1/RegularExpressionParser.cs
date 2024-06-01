using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Compilers_Lab1
{
    public class RegularExpressionParser
    {
        public string Data;
        public int Next;

        public void Init(string expression)
        {
            Data = PreprocessExpression(expression);
            Next = 0;
        }

        public char Peek()
        {
            return Next < Data.Length ? Data[Next] : '\0';
        }
        public char Pop()
        {
            var current = Peek();
            if (Next < Data.Length)
                Next++;
            return current;
        }

        public string PreprocessExpression(string expression)
        {
            StringBuilder result = new StringBuilder();

            var current = expression.GetEnumerator();
            var next = expression.GetEnumerator();

            next.MoveNext();

            while (next.MoveNext())
            {
                current.MoveNext();
                result.Append(current.Current);
                if ((char.IsLetterOrDigit(current.Current) || current.Current == ')' || current.Current == '*' ||
                    current.Current == '?') && (next.Current != ')' && next.Current != '|' &&
                    next.Current != '*' && next.Current != '?')) 
                    {
                        result.Append('@');
                    }
            }
            if (current.MoveNext())
            {
                result.Append(current.Current);
            }
            return result.ToString();
        }

        public static void PrintTree(ParseTree node, int offset)
        {
            if (node == null)
                return;

            for (int i = 0; i < offset; ++i)
                Console.Write(" ");

            switch (node.Type)
            {
                case ParseTreeNodeTypeEnum.Char:
                    Console.WriteLine(node.Data);
                    break;
                case ParseTreeNodeTypeEnum.Or:
                    Console.WriteLine("|");
                    break;
                case ParseTreeNodeTypeEnum.And:
                    Console.WriteLine("Concat");
                    break;
                case ParseTreeNodeTypeEnum.Question:
                    Console.WriteLine("?");
                    break;
                case ParseTreeNodeTypeEnum.Star:
                    Console.WriteLine("*");
                    break;
            }

            Console.Write("");

            PrintTree(node.Left, offset + 8);
            PrintTree(node.Right, offset + 8);
        }

        public ParseTree Char()
        {
            char data = Peek();

            if (char.IsLetterOrDigit(data) || data == '\0')
            {
                return new ParseTree(ParseTreeNodeTypeEnum.Char, this.Pop(), null, null);
            }
            else
            {
                Console.WriteLine($"Parse error: expected alphanumeric, got {0} at position {1}",
                Peek(), Next);
                Console.ReadKey();

                Environment.Exit(1);

                return null;
            }
        }

        public ParseTree Atom()
        {
            ParseTree atomNode;

            if (Peek() == '(')
            {
                Pop();

                atomNode = Expression();

                if (Pop() != ')')
                {
                    Console.WriteLine("Parse error: expected ')'");
                    throw new Exception();
                    Environment.Exit(1);
                }
            }
            else
                atomNode = Char();

            return atomNode;
        }

        public ParseTree Rep()
        {
            ParseTree atomNode = Atom();

            if (Peek() == '*')
            {
                Pop();

                ParseTree repNode = new ParseTree(ParseTreeNodeTypeEnum.Star, null, atomNode, null);

                return repNode;
            }
            else if (Peek() == '?')
            {
                Pop();

                ParseTree repNode = new ParseTree(ParseTreeNodeTypeEnum.Question, ' ', atomNode, null);

                return repNode;
            }
            else
                return atomNode;
        }

        public ParseTree Concat()
        {
            ParseTree left = Rep();

            if (Peek() == '@')
            {
                Pop();

                ParseTree right = Concat();

                ParseTree concatNode = new ParseTree(ParseTreeNodeTypeEnum.And, null, left, right);

                return concatNode;
            }
            else
                return left;
        }

        public ParseTree Expression()
        {
            ParseTree left = Concat();

            if (Peek() == '|')
            {
                Pop();

                ParseTree right = Expression();

                ParseTree exprNode = new ParseTree(ParseTreeNodeTypeEnum.Or, null, left, right);

                return exprNode;
            }
            else
                return left;
        }
    }
}
