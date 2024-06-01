using Compilers_Lab1;

namespace TestProject1
{
    public class UnitTest1
    {
        string[] strings1True = new[] { "zr", "ababbaazr", "aaaaaaaazr" };
        string[] strings1False = new[] { "zrazr", "t" };

        [Fact]
        public void TestRegex1()
        {
            var engine = new RegexEngineService("(a|b)*zr");
            foreach (var str in strings1True)
            {
                var answer = engine.Simulate(str);
                Assert.True(answer);
            }
            foreach (var str in strings1False)
            {
                var answer = engine.Simulate(str);
                Assert.False(answer);
            }
        }
    }
}