using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilers_Lab1
{
    public class NondeterministicFiniteAutomaton
    {
        const char epsilon = 'ε';
        public int InitState;
        public int FinalState;
        public int Size;
        public List<char> Inputs;
        public char[][] StateTable;

        public NondeterministicFiniteAutomaton(int initState, int finalState, int size)
        {
            InitState = initState;
            FinalState = finalState;
            Size = size;
            Inputs = new List<char>();
            StateTable = new char[size][];
            for (int i = 0; i < size; i++)
            {
                StateTable[i] = new char[size];
            }
        }

        public NondeterministicFiniteAutomaton()
        {
            InitState = 0;
            FinalState = 0;
            Size = 1;
            Inputs = new List<char>();
            StateTable = new char[Size][];
            for (int i = 0; i < Size; i++)
            {
                StateTable[i] = new char[Size];
            }
        }
        public NondeterministicFiniteAutomaton(NondeterministicFiniteAutomaton nfa)
        {
            InitState= nfa.InitState;
            FinalState= nfa.FinalState;
            Size = nfa.Size;
            Inputs = nfa.Inputs;
            StateTable = nfa.StateTable;
        }

        public void AddEdge(int from, int to, char input)
        {
            StateTable[from][to] = input;
            if (input != epsilon)
            {
                Inputs.Add(input);
            }
        }

        public void FillStateTable(NondeterministicFiniteAutomaton other)
        {
            for(int i = 0; i<other.Size; i++)
            {
                for(int j = 0; j < other.Size; j++)
                {
                    StateTable[i][j] = other.StateTable[i][j];
                }
            }
            foreach(var input in other.Inputs)
            {
                Inputs.Add(input);
            }
        }

        public void AddNewStatesOnLeft(int offset)
        {
            var newSize = Size + offset;
            if (offset < 0)
                return;
            char[][] newStateTable = new char[newSize][];
            for(int i = 0; i < newSize; i++)
            {
                newStateTable[i] = new char[newSize];
            }
            for(int i = 0; i < Size; i++)
            {
                for(int j =0; j< Size; j++)
                {
                    newStateTable[i + offset][j + offset] = StateTable[i][j];
                }
            }
            Size = newSize;
            StateTable = newStateTable;
            InitState += offset;
            FinalState += offset;
        }

        public void AddNewStatesOnRight(int offset)
        {
            var newSize = Size + offset;
            if (offset < 0)
                return;
            char[][] newStateTable = new char[newSize][];
            for (int i = 0; i < newSize; i++)
            {
                newStateTable[i] = new char[newSize];
            }
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    newStateTable[i][j] = StateTable[i][j];
                }
            }
            Size = newSize;
            StateTable = newStateTable;
        }

        public NondeterministicFiniteAutomaton BuildNFABasic(char input)
        {
            NondeterministicFiniteAutomaton basic = new NondeterministicFiniteAutomaton(0, 1, 2);

            basic.AddEdge(0, 1, input);

            return basic;
        }

        public NondeterministicFiniteAutomaton BuildNFAOr(NondeterministicFiniteAutomaton nfa1, NondeterministicFiniteAutomaton nfa2)
        {
            nfa1.AddNewStatesOnLeft(1);

            nfa2.AddNewStatesOnLeft(nfa1.Size);

            NondeterministicFiniteAutomaton newNFA = new NondeterministicFiniteAutomaton(nfa2);

            newNFA.FillStateTable(nfa1);

            newNFA.AddEdge(0, nfa1.InitState, epsilon);
            newNFA.AddEdge(0, nfa2.InitState, epsilon);

            newNFA.InitState = 0;

            newNFA.AddNewStatesOnRight(1);

            newNFA.FinalState = newNFA.Size - 1;

            newNFA.AddEdge(nfa1.FinalState, newNFA.FinalState, epsilon);
            newNFA.AddEdge(nfa2.FinalState, newNFA.FinalState, epsilon);

            return newNFA;
        }

        public NondeterministicFiniteAutomaton BuildNFAConcat(NondeterministicFiniteAutomaton nfa1, NondeterministicFiniteAutomaton nfa2)
        {
            nfa2.AddNewStatesOnLeft(nfa1.Size - 1);

            var newNFA = new NondeterministicFiniteAutomaton(nfa2);

            newNFA.FillStateTable(nfa1);

            newNFA.InitState = nfa1.InitState;

            return newNFA;
        }

        public NondeterministicFiniteAutomaton BuildNFAStar(NondeterministicFiniteAutomaton nfa)
        {
            
            nfa.AddNewStatesOnLeft(1);

           
            nfa.AddNewStatesOnRight(1);

            
            nfa.AddEdge(nfa.FinalState, nfa.InitState, epsilon);
            nfa.AddEdge(0, nfa.InitState, epsilon);
            nfa.AddEdge(nfa.FinalState, nfa.Size - 1, epsilon);
            nfa.AddEdge(0, nfa.Size - 1, epsilon);

            nfa.InitState = 0;
            nfa.FinalState = nfa.Size - 1;

            return nfa;
        }
        public NondeterministicFiniteAutomaton TreeToNFA(ParseTree tree)
        {
            switch (tree.Type)
            {
                case ParseTreeNodeTypeEnum.Char:
                    return BuildNFABasic(tree.Data.Value);
                case ParseTreeNodeTypeEnum.Or:
                    return BuildNFAOr(TreeToNFA(tree.Left), TreeToNFA(tree.Right));
                case ParseTreeNodeTypeEnum.And:
                    return BuildNFAConcat(TreeToNFA(tree.Left), TreeToNFA(tree.Right));
                case ParseTreeNodeTypeEnum.Star:
                    return BuildNFAStar(TreeToNFA(tree.Left));
                case ParseTreeNodeTypeEnum.Question:
                    return BuildNFAOr(TreeToNFA(tree.Left), BuildNFABasic(epsilon));
                default:
                    return null;
            }
        }

        public List<int> Move(List<int> states, char input)
        {
            var result = new List<int>();

            foreach (var state in states)
            {
                int i = 0;

                foreach (var chr in StateTable[state])
                {
                    if (chr == input)
                    {
                        var u = Array.IndexOf(StateTable[state], chr, i);
                        result.Add(u);
                    }

                    i = i + 1;
                }
            }

            return result;
        }
    }
}
