using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compilers_Lab1
{
    public static class DFAMinimizer
    {
        public static DeterministicFiniteAutomaton Minimize(DeterministicFiniteAutomaton dfa)
        {
            var statesCount = dfa.StatesTable.Values.Union(dfa.StatesTable.Keys.Select(x => x.Item1)).Distinct().Count();
            var markingTable = new bool[statesCount, statesCount];
            for (int i = 0; i < statesCount; i++)
            {
                for (int j = i; j < statesCount; j++)
                {
                    if(i == j)
                    {
                        markingTable[i, j] = true;
                        continue;
                    }
                    if((dfa.Final.Contains(i) && !dfa.Final.Contains(j)) || (dfa.Final.Contains(j) && !dfa.Final.Contains(i)))
                    {
                        markingTable[i, j] = true;
                    }
                    else
                    {
                        markingTable[i, j] = false;
                    }
                }
            }
            bool isChangeMade = true;
            while (isChangeMade)
            {
                isChangeMade = false;
                foreach (var input in dfa.Inputs)
                {
                    for (int i = 0; i < statesCount; i++)
                    {
                        for (int j = i; j < statesCount; j++)
                        {
                            if (markingTable[i, j])
                            {
                                continue;
                            }
                            if ((markingTable[dfa.Move(i, input), dfa.Move(j, input)] || markingTable[dfa.Move(j, input), dfa.Move(i, input)]) && dfa.Move(i, input) != dfa.Move(j, input))
                            {
                                markingTable[i, j] = true;
                                isChangeMade = true;
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < statesCount; i++)
            {
                for (int j = i; j < statesCount; j++)
                {
                    if(!markingTable[i, j])
                    {
                        var keys = dfa.StatesTable.Where(pair => pair.Value == j)
                            .Select(pair => pair.Key).ToArray();
                        foreach(var key in keys)
                        {
                            if(dfa.StatesTable.ContainsKey(key))
                            {
                                dfa.StatesTable[key] = i;
                                continue;
                            }
                            dfa.StatesTable.Add(key, i);
                        }
                        var keyValues = dfa.StatesTable.Where(pair => pair.Key.Item1 == j).ToArray();
                        foreach(var keyValue in keyValues)
                        {
                            if (dfa.StatesTable.ContainsKey((i, keyValue.Key.Item2)))
                            {
                                dfa.StatesTable[(i, keyValue.Key.Item2)] = keyValue.Value;
                                continue;
                            }
                            dfa.StatesTable.Add((i, keyValue.Key.Item2), keyValue.Value);
                        }
                        foreach(var input in dfa.Inputs)
                        {
                            dfa.StatesTable.Remove((j, input));
                            if(dfa.Final.Contains(j))
                            {
                                dfa.Final.Remove(j);
                                if (!dfa.Final.Contains(i))
                                {
                                    dfa.Final.Add(i);
                                }

                            }
                        }
                        var newDict = dfa.StatesTable.Where(pair => pair.Value != j).ToDictionary(key => key.Key, value => value.Value);
                        dfa.StatesTable = newDict;
                    }
                }
            }

            dfa.Start = dfa.StatesTable.Select(x => x.Key.Item1).OrderBy(x => x).First();
            return dfa;
        }

    }
}
