using System;

namespace Compilers_Lab1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string regex;
            regex = Console.ReadLine();
            if(regex == null)
            {
                Console.WriteLine("Parse error: regular expression was not provided");
                Environment.Exit(1);
            }

            regex.Append('\0');

            var parser = new RegularExpressionParser();

            parser.Init(regex);

            ParseTree parseTree = parser.Expression();

            if (parser.Peek() != '\0')
            {
                Console.WriteLine("Parse error: unexpected char, got {0} at #{1}",

                parser.Peek(), parser.Next);

                Environment.Exit(1);
            }

            var nfa = new NondeterministicFiniteAutomaton();
            nfa = nfa.TreeToNFA(parseTree);

            Console.WriteLine($"Initial State : {nfa.InitState}");
            Console.WriteLine($"Final State : {nfa.FinalState}");

            for (int i = 0; i < nfa.Size; i++)
            {
                for(int j = 0; j< nfa.Size; j++)
                {
                    if (nfa.StateTable[i][j] != '\0')
                    {
                        var outChar = nfa.StateTable[i][j] == 'ε' ? "epsilon" : nfa.StateTable[i][j].ToString();
                        Console.WriteLine($"Edge from {i} to {j} on input {outChar}");
                    }
                }
            }

            var translator = new NFAToDFATranslator();

            var dfa = translator.Translate(nfa);

            dfa.Show();

            var newdfa = DFAMinimizer.Minimize(dfa);

            Console.WriteLine();
            Console.WriteLine();

            newdfa.Show();

            while (true)
            {
                var input = Console.ReadLine();
                Console.WriteLine(newdfa.Simulate(input));
            }
        }
    }
}