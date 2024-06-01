using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilers_Lab1
{
    public class DeterministicFiniteAutomaton
    {
        public int Start;
        public List<int> Final;
        public Dictionary<(int, char), int> StatesTable;
        public List<char> Inputs;

        public DeterministicFiniteAutomaton()
        {
            Final = new List<int>();

            StatesTable = new Dictionary<(int, char), int>();
        }
        public void Show()
        {
            Console.WriteLine($"DFA start state: {Start}");
            Console.WriteLine($"DFA final state(s): {string.Join(", ", Final.Select(x=>x.ToString()))}");

            Console.Write("\n\n");

            var statesCount = StatesTable.Values.Union(StatesTable.Keys.Select(x => x.Item1)).OrderByDescending(x=>x).First();
            foreach (var kvp in StatesTable)
            {
                if(kvp.Key.Item1 != statesCount && kvp.Value != statesCount)
                    Console.WriteLine($"Trans[{kvp.Key.Item1}, {kvp.Key.Item2}] = {kvp.Value}");
            }
        }

        public int Move(int state, char input)
        {
            if (StatesTable.ContainsKey((state, input)))
                return StatesTable[(state, input)];
            return StatesTable[(state, '~')];
        }

        public bool Simulate(string input)
        {
            int currentState = Start;

            CharEnumerator i = input.GetEnumerator();

            while (i.MoveNext())
            {
                (int, char) transition = (currentState, i.Current);

                if (!StatesTable.ContainsKey(transition))
                    return false;

                currentState = StatesTable[transition];
            }

            if (Final.Contains(currentState))
                return true;
            else
                return false;
        }
    }
}
