using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilers_Lab1
{
    public class RegexEngineService
    {
        public DeterministicFiniteAutomaton Dfa;
        public RegexEngineService(string regex)
        {
            regex.Append('\0');

            var parser = new RegularExpressionParser();

            parser.Init(regex);

            ParseTree parseTree = parser.Expression();

            var nfa = new NondeterministicFiniteAutomaton();
            nfa = nfa.TreeToNFA(parseTree);

            var translator = new NFAToDFATranslator();

            var dfa = translator.Translate(nfa);

            var newdfa = DFAMinimizer.Minimize(dfa);

            Dfa = newdfa;
        }

        public bool Simulate(string input)
        {
            return Dfa.Simulate(input);
        }
    }
}
