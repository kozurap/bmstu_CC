using Compilers_Lab1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject1
{
    public class UnitTest3
    {
        string[] strings3True = new[] { "ab", "abzabzab", "abzab" };
        string[] strings3False = new[] { "ababz", "r" };

        [Fact]
        public void TestRegex3()
        {
            var engine = new RegexEngineService("(abz)*(abz)*ab");
            foreach (var str in strings3True)
            {
                var answer = engine.Simulate(str);
                Assert.True(answer);
            }
            foreach (var str in strings3False)
            {
                var answer = engine.Simulate(str);
                Assert.False(answer);
            }
        }
    }
}
