using Compilers_Lab1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public class UnitTest4
    {
        string[] strings4True = new[] { "aa", "aabba", "bb" };
        string[] strings4False = new[] { "aabbab", "aavvvv" };
        [Fact]
        public void TestRegex4()
        {
            var engine = new RegexEngineService("((a*bb(a|b)?)|a*)");
            foreach (var str in strings4True)
            {
                var answer = engine.Simulate(str);
                Assert.True(answer);
            }
            foreach (var str in strings4False)
            {
                var answer = engine.Simulate(str);
                Assert.False(answer);
            }
        }
    }
}
