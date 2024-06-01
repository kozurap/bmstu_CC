using Compilers_Lab1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{

    public class UnitTest2
    {
        string[] strings2True = new[] { "ababbaabbbabbbaaababbb", "ababbaabbbabbbaaababb", "ababbaabbbabbbaaababbbb" };
        string[] strings2False = new[] { "ababbaabbbabbbaaabab", "x" };

        [Fact]
        public void TestRegex2()
        {
            var engine = new RegexEngineService("(a|b)*a?b*(a|b)?b*(aa)(aba)*b?bbb?");
            foreach (var str in strings2True)
            {
                var answer = engine.Simulate(str);
                Assert.True(answer);
            }
            foreach (var str in strings2False)
            {
                var answer = engine.Simulate(str);
                Assert.False(answer);
            }
        }
    }
}
