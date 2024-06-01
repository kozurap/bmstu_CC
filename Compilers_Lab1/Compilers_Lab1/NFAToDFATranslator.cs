using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilers_Lab1
{
    public class NFAToDFATranslator
    {
        const char epsilon = 'ε';
        private int LastState = 0;
        public DeterministicFiniteAutomaton Translate(NondeterministicFiniteAutomaton nfa)
        {
            DeterministicFiniteAutomaton dfa = new DeterministicFiniteAutomaton();

            var markedStates = new List<List<int>>();
            var unmarkedStates = new Stack<List<int>>();

            var dfaStateNum = new Dictionary<List<int>, int>();

            var nfaInitial = new List<int>();
            nfaInitial.Add(nfa.InitState);

            List<int> first = EpsilonClosure(nfa, nfaInitial);
            unmarkedStates.Push(first);

            int dfaInitial = LastState++;
            dfaStateNum[first] = dfaInitial;
            dfa.Start = dfaInitial;
            var knownTerminals = nfa.Inputs.Distinct();

            while (unmarkedStates.Count != 0)
            {
                var unmarkedState = unmarkedStates.Pop();

                markedStates.Add(unmarkedState);

                if (unmarkedState.Contains(nfa.FinalState))
                    dfa.Final.Add(dfaStateNum[unmarkedState]);

                foreach (var terminal in knownTerminals)
                {
                    var next = EpsilonClosure(nfa, nfa.Move(unmarkedState, terminal));
                    if (next.Count != 0)
                    {
                        if (!unmarkedStates.Any(x => ListsIntsAreEqual(x, next)) && !markedStates.Any(x => ListsIntsAreEqual(x, next)))
                        {
                            unmarkedStates.Push(next);
                            dfaStateNum.Add(next, LastState++);
                        }

                        var transition = (dfaStateNum[unmarkedState], terminal);

                        dfa.StatesTable[transition] = dfaStateNum.First(x=> ListsIntsAreEqual(x.Key, next)).Value;
                    }
                }
            }
            dfa.Inputs = nfa.Inputs.Distinct().ToList();
            dfa.Inputs.Add('~');

            var statesCount = dfa.StatesTable.Values.Union(dfa.StatesTable.Keys.Select(x=>x.Item1)).OrderByDescending(x=>x).First()+1;
            for(int i =0; i < statesCount; i++)
            {
                dfa.StatesTable.Add((i, '~'), statesCount);
            }
            foreach(var input in dfa.Inputs)
            {
                dfa.StatesTable.Add((statesCount, input), statesCount);
            }

            LastState = 0;
            return dfa;
        }

        static List<int> EpsilonClosure(NondeterministicFiniteAutomaton nfa, List<int> states)
        {
            Stack<int> uncheckedStack = new Stack<int>(states);

            var epsilonClosure = states;

            while (uncheckedStack.Count != 0)
            {
                var t = uncheckedStack.Pop();

                int i = 0;

                foreach (var input in nfa.StateTable[t])
                {
                    if (input == epsilon)
                    {
                        int u = Array.IndexOf(nfa.StateTable[t], input, i);
                        if (!epsilonClosure.Contains(u))
                        {
                            epsilonClosure.Add(u);
                            uncheckedStack.Push(u);
                        }
                    }

                    i = i + 1;
                }
            }

            return epsilonClosure;
        }

        private static bool ListsIntsAreEqual(List<int> list1, List<int> list2)
        {
            if(list1.Count != list2.Count)
            {
                return false;
            }
            for (int i = 0; i < list1.Count; i++)
            {
                if (list1[i] != list2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
